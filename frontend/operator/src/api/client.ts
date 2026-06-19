import { api } from '@/lib/axios';
import * as T from './types';

// Routes reconciled against src/PrintPlatform.API/Controllers/*.cs.
//   ✓ route:           verified against an existing controller action
//   TODO(backend):     no controller action exists yet — path is the planned contract
export const apiClient = {
  auth: {
    // TODO(backend): no AuthController yet — planned: POST /api/auth/login
    login: async (command: T.LoginCommand): Promise<T.AuthResultDto> => {
      const response = await api.post<T.AuthResultDto>('/api/auth/login', command);
      return response.data;
    },
  },

  quotes: {
    // TODO(backend): no QuotesController yet — planned: GET /api/quotes/pending
    getPending: async (): Promise<T.PendingQuoteDto[]> => {
      const response = await api.get<T.PendingQuoteDto[]>('/api/quotes/pending');
      return response.data;
    },

    // TODO(backend): no QuotesController yet — planned: PUT /api/quotes/{id}/confirm
    confirm: async (command: T.ConfirmQuoteCommand): Promise<T.QuoteDto> => {
      const response = await api.put<T.QuoteDto>(`/api/quotes/${command.quoteId}/confirm`, command);
      return response.data;
    },
  },

  dispatch: {
    // ✓ route: GET /api/dispatch/pending
    getPending: async (): Promise<T.JobAssignmentDto[]> => {
      const response = await api.get<T.JobAssignmentDto[]>('/api/dispatch/pending');
      return response.data;
    },

    // ✓ route: POST /api/dispatch/assign
    assign: async (request: T.AssignJobRequest): Promise<T.JobAssignmentDto> => {
      const response = await api.post<T.JobAssignmentDto>('/api/dispatch/assign', request);
      return response.data;
    },

    // ✓ route: GET /api/dispatch/my-jobs
    getJobs: async (): Promise<T.JobAssignmentDto[]> => {
      const response = await api.get<T.JobAssignmentDto[]>('/api/dispatch/my-jobs');
      return response.data;
    },

    // TODO(backend): DispatchController has no qc/approve action yet — planned: POST /api/dispatch/{id}/qc/approve
    qcApprove: async (command: T.ApproveQcCommand): Promise<void> => {
      await api.post(`/api/dispatch/${command.jobAssignmentId}/qc/approve`, command);
    },

    // TODO(backend): DispatchController has no qc/reject action yet — planned: POST /api/dispatch/{id}/qc/reject
    qcReject: async (command: T.RejectQcCommand): Promise<void> => {
      await api.post(`/api/dispatch/${command.jobAssignmentId}/qc/reject`, command);
    },
  },

  printers: {
    // ✓ route: GET /api/printers/available-for-job/{jobId}
    availableForJob: async (jobId: string): Promise<T.RankedPrinterDto[]> => {
      const response = await api.get<T.RankedPrinterDto[]>(`/api/printers/available-for-job/${jobId}`);
      return response.data;
    },
  },

  finance: {
    // ✓ route: GET /api/finance/payouts
    getPayouts: async (): Promise<T.PayoutDto[]> => {
      const response = await api.get<T.PayoutDto[]>('/api/finance/payouts');
      return response.data;
    },

    // ✓ route: POST /api/finance/payouts/{id}/mark-sent
    markSent: async (id: string): Promise<void> => {
      await api.post(`/api/finance/payouts/${id}/mark-sent`);
    },

    // ✓ route: GET /api/finance/platform/revenue
    getRevenue: async (): Promise<T.PlatformRevenueDto> => {
      const response = await api.get<T.PlatformRevenueDto>('/api/finance/platform/revenue');
      return response.data;
    },
  },
};
