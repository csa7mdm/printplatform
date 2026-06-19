import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import * as T from './types';

export const queryKeys = {
  quotes: {
    pending: () => ['quotes', 'pending'] as const,
  },
  dispatch: {
    pending: () => ['dispatch', 'pending'] as const,
    jobs: () => ['dispatch', 'jobs'] as const,
  },
  printers: {
    availableForJob: (jobId: string) => ['printers', 'available-for-job', jobId] as const,
  },
  finance: {
    payouts: () => ['finance', 'payouts'] as const,
    revenue: () => ['finance', 'revenue'] as const,
  },
};

// --- Auth ---

export function useLogin() {
  return useMutation({
    mutationFn: (command: T.LoginCommand) => apiClient.auth.login(command),
    onSuccess: (data) => {
      localStorage.setItem('operator_token', data.accessToken);
    },
  });
}

// --- Quotes ---

export function usePendingQuotes() {
  return useQuery({
    queryKey: queryKeys.quotes.pending(),
    queryFn: () => apiClient.quotes.getPending(),
  });
}

export function useConfirmQuote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.ConfirmQuoteCommand) => apiClient.quotes.confirm(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.quotes.pending() });
    },
  });
}

// --- Dispatch ---

export function usePendingDispatch() {
  return useQuery({
    queryKey: queryKeys.dispatch.pending(),
    queryFn: () => apiClient.dispatch.getPending(),
  });
}

export function useAssignJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: T.AssignJobRequest) => apiClient.dispatch.assign(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.dispatch.pending() });
      queryClient.invalidateQueries({ queryKey: queryKeys.dispatch.jobs() });
    },
  });
}

export function useQcApprove() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.ApproveQcCommand) => apiClient.dispatch.qcApprove(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.dispatch.jobs() });
    },
  });
}

export function useQcReject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.RejectQcCommand) => apiClient.dispatch.qcReject(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.dispatch.jobs() });
    },
  });
}

export function useDispatchJobs() {
  return useQuery({
    queryKey: queryKeys.dispatch.jobs(),
    queryFn: () => apiClient.dispatch.getJobs(),
  });
}

// --- Printers ---

export function useAvailablePrintersForJob(jobId: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: queryKeys.printers.availableForJob(jobId),
    queryFn: () => apiClient.printers.availableForJob(jobId),
    ...options,
  });
}

// --- Finance ---

export function usePayouts() {
  return useQuery({
    queryKey: queryKeys.finance.payouts(),
    queryFn: () => apiClient.finance.getPayouts(),
  });
}

export function useMarkPayoutSent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.finance.markSent(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.finance.payouts() });
      queryClient.invalidateQueries({ queryKey: queryKeys.finance.revenue() });
    },
  });
}

export function usePlatformRevenue() {
  return useQuery({
    queryKey: queryKeys.finance.revenue(),
    queryFn: () => apiClient.finance.getRevenue(),
  });
}
