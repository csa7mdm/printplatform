import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import * as T from './types';

// Query Keys
export const queryKeys = {
  auth: {
    me: () => ['auth', 'me'] as const,
  },
  models: {
    all: () => ['models'] as const,
    detail: (id: string) => ['models', id] as const,
  },
  quotes: {
    all: () => ['quotes'] as const,
    detail: (id: string) => ['quotes', id] as const,
  },
  orders: {
    all: () => ['orders'] as const,
    detail: (id: string) => ['orders', id] as const,
  },
  materials: {
    all: (activeOnly?: boolean) => ['materials', { activeOnly }] as const,
    detail: (id: string) => ['materials', id] as const,
  },
  loyalty: {
    info: () => ['loyalty'] as const,
  },
};

// --- Auth Hooks ---

export function useRegisterCustomer() {
  return useMutation({
    mutationFn: (command: T.RegisterCustomerCommand) => apiClient.auth.registerCustomer(command),
  });
}

export function useLogin() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.LoginCommand) => apiClient.auth.login(command),
    onSuccess: (data) => {
      localStorage.setItem('token', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      queryClient.invalidateQueries({ queryKey: queryKeys.auth.me() });
    },
  });
}

export function useVerifyOtp() {
  return useMutation({
    mutationFn: (command: T.VerifyPhoneOtpCommand) => apiClient.auth.verifyOtp(command),
  });
}

export function useCurrentUser(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: queryKeys.auth.me(),
    queryFn: () => apiClient.auth.getCurrentUser(),
    ...options,
  });
}

// --- Models Hooks ---

export function useUploadModel() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (file: File) => apiClient.models.upload(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.models.all() });
    },
  });
}

export function useModelDetails(id: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: queryKeys.models.detail(id),
    queryFn: () => apiClient.models.get(id),
    ...options,
  });
}

// --- Quotes Hooks ---

export function useCreateQuoteRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.CreateQuoteRequestCommand) => apiClient.quotes.create(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.quotes.all() });
    },
  });
}

export function useQuotes() {
  return useQuery({
    queryKey: queryKeys.quotes.all(),
    queryFn: () => apiClient.quotes.list(),
  });
}

export function useQuoteDetails(id: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: queryKeys.quotes.detail(id),
    queryFn: () => apiClient.quotes.get(id),
    ...options,
  });
}

export function useAcceptQuote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.AcceptQuoteCommand) => apiClient.quotes.accept(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.quotes.all() });
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all() });
    },
  });
}

// --- Orders Hooks ---

export function useOrders() {
  return useQuery({
    queryKey: queryKeys.orders.all(),
    queryFn: () => apiClient.orders.list(),
  });
}

export function useOrderDetails(id: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: queryKeys.orders.detail(id),
    queryFn: () => apiClient.orders.get(id),
    ...options,
  });
}

export function useInitiatePayment() {
  return useMutation({
    mutationFn: (command: T.InitiatePaymentCommand) => apiClient.orders.initiatePayment(command),
  });
}

export function useCancelOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: T.CancelOrderCommand) => apiClient.orders.cancel(command),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all() });
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(variables.orderId) });
    },
  });
}

// --- Materials Hooks ---

export function useMaterials(activeOnly = true) {
  return useQuery({
    queryKey: queryKeys.materials.all(activeOnly),
    queryFn: () => apiClient.materials.list(activeOnly),
  });
}

export function useMaterialDetails(id: string) {
  return useQuery({
    queryKey: queryKeys.materials.detail(id),
    queryFn: () => apiClient.materials.get(id),
  });
}

// --- Loyalty Hooks ---

export function useLoyaltyInfo() {
  return useQuery({
    queryKey: queryKeys.loyalty.info(),
    queryFn: () => apiClient.loyalty.get(),
  });
}
