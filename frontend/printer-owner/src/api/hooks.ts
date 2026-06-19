import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import * as T from './types';

export const qk = {
  myPrinters: ['printers', 'mine'] as const,
  myJobs: ['dispatch', 'my-jobs'] as const,
  earnings: ['finance', 'my-earnings'] as const,
  earningsBreakdown: ['finance', 'my-earnings', 'breakdown'] as const,
  achievements: ['achievements', 'me'] as const,
};

// -- Auth / onboarding -------------------------------------------------------
export const useRegister = () =>
  useMutation({ mutationFn: (c: T.RegisterPrinterOwnerCommand) => apiClient.auth.register(c) });

export const useLogin = () =>
  useMutation({ mutationFn: (c: T.LoginCommand) => apiClient.auth.login(c) });

export const useVerifyOtp = () =>
  useMutation({ mutationFn: (c: T.VerifyPhoneOtpCommand) => apiClient.auth.verifyOtp(c) });

export const useCompleteOnboarding = () =>
  useMutation({ mutationFn: (c: T.CompleteOnboardingCommand) => apiClient.profile.completeOnboarding(c) });

// -- Printers ----------------------------------------------------------------
export const useMyPrinters = () =>
  useQuery({ queryKey: qk.myPrinters, queryFn: apiClient.printers.mine });

export const useAddPrinter = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (c: T.AddPrinterCommand) => apiClient.printers.add(c),
    onSuccess: () => qc.invalidateQueries({ queryKey: qk.myPrinters }),
  });
};

export const useSetPrinterAvailability = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isAvailable }: { id: string; isAvailable: boolean }) =>
      apiClient.printers.setAvailability(id, isAvailable),
    onSuccess: () => qc.invalidateQueries({ queryKey: qk.myPrinters }),
  });
};

// -- Jobs --------------------------------------------------------------------
export const useMyJobs = () =>
  useQuery({ queryKey: qk.myJobs, queryFn: apiClient.jobs.mine });

const useJobMutation = <TArgs>(fn: (a: TArgs) => Promise<void>) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: fn,
    onSuccess: () => qc.invalidateQueries({ queryKey: qk.myJobs }),
  });
};

export const useAcceptJob = () => useJobMutation((id: string) => apiClient.jobs.accept(id));
export const useRejectJob = () =>
  useJobMutation(({ id, reason }: { id: string; reason: string }) => apiClient.jobs.reject(id, reason));
export const useStartJob = () => useJobMutation((id: string) => apiClient.jobs.start(id));
export const useUploadCompletion = () =>
  useJobMutation(({ id, photos }: { id: string; photos: File[] }) => apiClient.jobs.uploadCompletion(id, photos));

// -- Earnings ----------------------------------------------------------------
export const useEarnings = () =>
  useQuery({ queryKey: qk.earnings, queryFn: apiClient.earnings.summary });

export const useEarningsBreakdown = () =>
  useQuery({ queryKey: qk.earningsBreakdown, queryFn: apiClient.earnings.breakdown });

// -- Achievements (TODO(backend): endpoint not implemented yet) ---------------
export const useAchievements = () =>
  useQuery({ queryKey: qk.achievements, queryFn: apiClient.achievements.get });
