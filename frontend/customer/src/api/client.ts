import { api } from '@/lib/axios';
import * as T from './types';

export const apiClient = {
  auth: {
    // TODO(backend): controller not implemented yet — route is the planned contract
    registerCustomer: async (command: T.RegisterCustomerCommand): Promise<T.RegistrationResultDto> => {
      const response = await api.post<T.RegistrationResultDto>('/auth/register/customer', command);
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    login: async (command: T.LoginCommand): Promise<T.AuthResultDto> => {
      const response = await api.post<T.AuthResultDto>('/auth/login', command);
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    verifyOtp: async (command: T.VerifyPhoneOtpCommand): Promise<void> => {
      await api.post('/auth/verify-otp', command);
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    getCurrentUser: async (): Promise<T.CurrentUserDto> => {
      const response = await api.get<T.CurrentUserDto>('/profile/me');
      return response.data;
    },
  },

  models: {
    // TODO(backend): controller not implemented yet — route is the planned contract
    upload: async (file: File): Promise<T.ModelFileDto> => {
      const formData = new FormData();
      formData.append('file', file);
      const response = await api.post<T.ModelFileDto>('/models/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    get: async (id: string): Promise<T.ModelFileDto> => {
      const response = await api.get<T.ModelFileDto>(`/models/${id}`);
      return response.data;
    },
  },

  quotes: {
    // TODO(backend): controller not implemented yet — route is the planned contract
    create: async (command: T.CreateQuoteRequestCommand): Promise<string> => {
      const response = await api.post<{ id: string }>('/quotes/request', command);
      return response.data.id;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    list: async (): Promise<T.PendingQuoteDto[]> => {
      const response = await api.get<T.PendingQuoteDto[]>('/quotes');
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    get: async (id: string): Promise<T.QuoteDto> => {
      const response = await api.get<T.QuoteDto>(`/quotes/${id}`);
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    accept: async (command: T.AcceptQuoteCommand): Promise<T.OrderDetailsDto> => {
      const response = await api.post<T.OrderDetailsDto>(`/quotes/${command.quoteId}/accept`, command);
      return response.data;
    },
  },

  orders: {
    // TODO(backend): controller not implemented yet — route is the planned contract
    list: async (): Promise<T.OrderSummaryDto[]> => {
      const response = await api.get<T.OrderSummaryDto[]>('/orders');
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    get: async (id: string): Promise<T.OrderDetailsDto> => {
      const response = await api.get<T.OrderDetailsDto>(`/orders/${id}`);
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    initiatePayment: async (command: T.InitiatePaymentCommand): Promise<T.InitiatePaymentResponse> => {
      const response = await api.post<T.InitiatePaymentResponse>(`/orders/${command.orderId}/payment/initiate`, command);
      return response.data;
    },

    // TODO(backend): controller not implemented yet — route is the planned contract
    cancel: async (command: T.CancelOrderCommand): Promise<void> => {
      await api.post(`/orders/${command.orderId}/cancel`, { reason: command.reason });
    },
  },

  materials: {
    // Verified backend route: GET /api/materials
    list: async (activeOnly = true): Promise<T.MaterialOptionDto[]> => {
      const response = await api.get<T.MaterialOptionDto[]>('/materials', {
        params: { activeOnly },
      });
      return response.data;
    },
    
    // Verified backend route: GET /api/materials/{id}
    get: async (id: string): Promise<T.MaterialOptionDto> => {
      const response = await api.get<T.MaterialOptionDto>(`/materials/${id}`);
      return response.data;
    },
  },

  loyalty: {
    // TODO(backend): controller not implemented yet — route is the planned contract (loyalty stub)
    get: async (): Promise<T.LoyaltyInfoDto> => {
      const response = await api.get<T.LoyaltyInfoDto>('/loyalty');
      return response.data;
    },
  },
};
