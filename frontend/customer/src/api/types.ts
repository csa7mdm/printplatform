export enum UserRole {
  Customer = 0,
  PrinterOwner = 1,
  Operator = 2,
  Admin = 3,
}

export enum Lang {
  Arabic = 0,
  English = 1,
}

export enum FileFormat {
  Stl = 1,
  ThreeMF = 2,
  Obj = 3,
}

export enum ModelFileStatus {
  Uploaded = 0,
  AnalysisPending = 1,
  Approved = 2,
  Rejected = 3,
}

export enum QualityPreset {
  Draft = 0,
  Standard = 1,
  Fine = 2,
  Ultra = 3,
}

export enum QuoteRequestStatus {
  PendingReview = 0,
  Quoted = 1,
  Expired = 2,
  Accepted = 3,
  Rejected = 4,
}

export enum QuoteStatus {
  Draft = 0,
  Sent = 1,
  Accepted = 2,
  Expired = 3,
}

export enum OrderStatus {
  PendingPayment = 0,
  Confirmed = 1,
  InProduction = 2,
  QCPending = 3,
  ReadyToShip = 4,
  Shipped = 5,
  Delivered = 6,
  Cancelled = 7,
  Refunded = 8,
}

export enum PaymentMethod {
  COD = 0,
  InstaPay = 1,
  VodafoneCash = 2,
  Card = 3,
  BankTransfer = 4,
}

export enum PaymentStatus {
  Pending = 0,
  Paid = 1,
  PartiallyPaid = 2,
  Refunded = 3,
}

export enum MaterialType {
  PLA = 0,
  PETG = 1,
  ABS = 2,
  TPU = 3,
  Nylon = 4,
  PLA_CF = 5,
  Resin_Standard = 6,
  Resin_Dental = 7,
  Resin_Engineering = 8,
}

export interface RegisterCustomerCommand {
  email: string;
  phoneNumber: string;
  password: string;
  fullNameAr: string;
  fullNameEn: string;
  lang: Lang;
}

export interface VerifyPhoneOtpCommand {
  userId: string;
  code: string;
}

export interface LoginCommand {
  email: string;
  password: string;
}

export interface AuthResultDto {
  userId: string;
  email: string;
  role: UserRole;
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface RegistrationResultDto {
  userId: string;
  email: string;
  phoneNumber: string;
  role: UserRole;
  otpSent: boolean;
}

export interface CurrentUserDto {
  userId: string;
  email: string;
  phoneNumber: string;
  fullNameAr: string;
  fullNameEn: string;
  role: UserRole;
  lang: Lang;
  isActive: boolean;
  isVerified: boolean;
}

export interface ModelFileDto {
  id: string;
  customerUserId: string;
  originalFileName: string;
  fileSizeBytes: number;
  fileFormat: FileFormat;
  status: ModelFileStatus;
  estimatedVolumeCC?: number;
  boundingBoxX?: number;
  boundingBoxY?: number;
  boundingBoxZ?: number;
  estimatedWeightGrams?: number;
  estimatedPrintHours?: number;
  analysisNotes?: string;
  createdAt: string;
}

export interface CreateQuoteRequestCommand {
  modelFileId: string;
  materialOptionId: string;
  qualityPreset: QualityPreset;
  layerHeightMm: number;
  infillPercent: number;
  quantity: number;
  includeDesignService: boolean;
}

export interface ShippingAddress {
  recipientName: string;
  phone: string;
  street: string;
  city: string;
  governorate: string;
  country: string;
  postalCode?: string | null;
  notes?: string | null;
}

export interface AcceptQuoteCommand {
  quoteId: string;
  paymentMethod: PaymentMethod;
  shippingAddress: ShippingAddress;
  discountEgp?: number;
  loyaltyPointsUsed?: number;
  loyaltyDiscountEgp?: number;
}

export interface OrderItemDto {
  id: string;
  quoteId: string;
  quantity: number;
  unitPriceEgp: number;
  totalEgp: number;
}

export interface OrderSummaryDto {
  id: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  totalEgp: number;
  createdAt: string;
}

export interface OrderDetailsDto {
  id: string;
  customerUserId: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  paymobOrderId?: string;
  shippingAddressSnapshot: string; // JSON string
  subtotalEgp: number;
  deliveryFeeEgp: number;
  discountEgp: number;
  loyaltyDiscountEgp: number;
  loyaltyPointsUsed: number;
  totalEgp: number;
  items: OrderItemDto[];
  createdAt: string;
}

export interface InitiatePaymentCommand {
  orderId: string;
  customerFirstName: string;
  customerLastName: string;
  customerEmail: string;
  customerPhone: string;
}

export interface InitiatePaymentResponse {
  paymobOrderId: string;
  paymentKey: string;
  iframeUrl: string;
}

export interface CancelOrderCommand {
  orderId: string;
  reason: string;
}

export interface MaterialOptionDto {
  id: string;
  materialType: MaterialType;
  colorName: string;
  colorHex: string;
  pricePerGram: number;
  ownerCostPerGram: number;
  propertiesJson: string;
  isActive: boolean;
}

export interface QuoteDto {
  id: string;
  quoteRequestId: string;
  estimatedWeightGrams: number;
  estimatedPrintHours: number;
  customerPriceEgp: number;
  designServicePriceEgp: number;
  deliveryEstimateEgp: number;
  totalPriceEgp: number;
  estimatedDeliveryDate?: string;
  validUntil: string;
  operatorNotes?: string;
  status: QuoteStatus;
}

export interface PendingQuoteDto {
  quoteRequestId: string;
  modelFileId: string;
  customerUserId: string;
  materialOptionId: string;
  qualityPreset: QualityPreset;
  quantity: number;
  includeDesignService: boolean;
  status: QuoteRequestStatus;
  createdAt: string;
}

// Loyalty Info (Stubbed)
export interface LoyaltyInfoDto {
  pointsBalance: number;
  tierName: string;
  tierDiscountPercent: number;
  totalSpentEgp: number;
}
