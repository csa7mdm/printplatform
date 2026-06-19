import { api } from '@/lib/axios';
import * as T from './types';

// baseURL already includes /api (see src/lib/axios.ts), so paths omit the /api prefix.
// Routes reconciled against src/PrintPlatform.API/Controllers/*.cs.
//   ✓ route:        verified against an existing controller action
//   TODO(backend):  no controller action exists yet — path is the planned contract
export const apiClient = {
  auth: {
    // ✓ route: POST /api/auth/register/printer-owner
    register: async (command: T.RegisterPrinterOwnerCommand): Promise<T.AuthResultDto> => {
      const res = await api.post<T.AuthResultDto>('/auth/register/printer-owner', command);
      return res.data;
    },
    // ✓ route: POST /api/auth/login
    login: async (command: T.LoginCommand): Promise<T.AuthResultDto> => {
      const res = await api.post<T.AuthResultDto>('/auth/login', command);
      return res.data;
    },
    // ✓ route: POST /api/auth/verify-otp
    verifyOtp: async (command: T.VerifyPhoneOtpCommand): Promise<void> => {
      await api.post('/auth/verify-otp', command);
    },
  },

  profile: {
    // ✓ route: PUT /api/profile/printer-owner/onboarding
    completeOnboarding: async (command: T.CompleteOnboardingCommand): Promise<void> => {
      await api.put('/profile/printer-owner/onboarding', command);
    },
  },

  printers: {
    // ✓ route: GET /api/printers/mine
    mine: async (): Promise<T.PrinterDto[]> => {
      const res = await api.get<T.PrinterDto[]>('/printers/mine');
      return res.data;
    },
    // ✓ route: POST /api/printers
    add: async (command: T.AddPrinterCommand): Promise<T.PrinterDto> => {
      const res = await api.post<T.PrinterDto>('/printers', command);
      return res.data;
    },
    // ✓ route: PUT /api/printers/{id}/availability
    setAvailability: async (id: string, isAvailable: boolean): Promise<void> => {
      await api.put(`/printers/${id}/availability`, { isAvailable });
    },
  },

  jobs: {
    // ✓ route: GET /api/dispatch/my-jobs
    mine: async (): Promise<T.JobAssignmentDto[]> => {
      const res = await api.get<T.JobAssignmentDto[]>('/dispatch/my-jobs');
      return res.data;
    },
    // ✓ route: POST /api/dispatch/{id}/accept
    accept: async (id: string): Promise<void> => {
      await api.post(`/dispatch/${id}/accept`);
    },
    // ✓ route: POST /api/dispatch/{id}/reject
    reject: async (id: string, reason: string): Promise<void> => {
      await api.post(`/dispatch/${id}/reject`, { reason });
    },
    // TODO(backend): DispatchController has no 'start' action yet — planned: POST /api/dispatch/{id}/start
    start: async (id: string): Promise<void> => {
      await api.post(`/dispatch/${id}/start`);
    },
    // TODO(backend): DispatchController has no 'complete' action yet — planned: POST /api/dispatch/{id}/complete (multipart photos)
    uploadCompletion: async (id: string, photos: File[]): Promise<void> => {
      const form = new FormData();
      photos.forEach((p) => form.append('photos', p));
      await api.post(`/dispatch/${id}/complete`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
  },

  earnings: {
    // ✓ route: GET /api/finance/my-earnings
    summary: async (): Promise<T.OwnerEarningsDto> => {
      const res = await api.get<T.OwnerEarningsDto>('/finance/my-earnings');
      return res.data;
    },
    // ✓ route: GET /api/finance/my-earnings/breakdown
    breakdown: async (): Promise<T.EarningsBreakdownItem[]> => {
      const res = await api.get<T.EarningsBreakdownItem[]>('/finance/my-earnings/breakdown');
      return res.data;
    },
  },

  achievements: {
    // TODO(backend): no gamification read controller yet — planned: GET /api/achievements/me
    get: async (): Promise<T.AchievementsDto> => {
      const res = await api.get<T.AchievementsDto>('/achievements/me');
      return res.data;
    },
  },
};
