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

export enum TechnologyType {
  FDM = 0,
  MSLA_Resin = 1,
  SLA = 2,
  SLS = 3,
}

export enum PrinterStatus {
  PendingVerification = 0,
  Active = 1,
  Paused = 2,
  Decommissioned = 3,
}

export enum JobAssignmentStatus {
  Offered = 1,
  Accepted = 2,
  Rejected = 3,
  Printing = 4,
  QCPending = 5,
  QCApproved = 6,
  QCRejected = 7,
  Ready = 8,
  Shipped = 9,
  Delivered = 10,
  Failed = 11,
}

export enum PayoutStatus {
  Pending = 0,
  Batched = 1,
  Paid = 2,
}

export enum OperatorDecision {
  Approved = 0,
  Rejected = 1,
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

export interface ConfirmQuoteCommand {
  quoteRequestId: string;
  overrideCustomerPriceEgp?: number;
  overrideDesignServicePriceEgp?: number;
  overrideDeliveryEstimateEgp?: number;
  overrideValidUntil?: string;
  operatorNotes?: string;
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

export interface JobAssignmentDto {
  id: string;
  orderItemId: string;
  printerId: string;
  printerOwnerUserId: string;
  status: JobAssignmentStatus;
  offeredAt: string;
  acceptedAt?: string;
  printingStartedAt?: string;
  completedAt?: string;
  payoutAmount: number;
  payoutStatus: PayoutStatus;
  operatorNotes?: string;
}

export interface AssignJobRequest {
  orderItemId: string;
  printerId: string;
  printerOwnerUserId: string;
  payoutAmount: number;
  operatorNotes?: string;
}

export interface ApproveQcCommand {
  jobAssignmentId: string;
  notes: string;
  photos: string[];
  checklist: string;
}

export interface RejectQcCommand {
  jobAssignmentId: string;
  notes: string;
  photos: string[];
  checklist: string;
  needsReprint: boolean;
}

export interface PrinterDto {
  id: string;
  printerOwnerProfileId: string;
  brand: string;
  model: string;
  technologyType: TechnologyType;
  buildVolumeX: number;
  buildVolumeY: number;
  buildVolumeZ: number;
  nozzleDiameter?: number;
  supportedMaterialsJson: string;
  maxLayerHeight: number;
  minLayerHeight: number;
  areaDistrict: string;
  areaCity: string;
  isDeliverable: boolean;
  status: PrinterStatus;
  certificationLevel: number;
  maxConcurrentJobs: number;
}

export interface RankedPrinterDto {
  printer: PrinterDto;
  score: number;
  capabilityMatch: number;
  proximityScore: number;
  certificationScore: number;
  loadScore: number;
  distanceKm: number;
}

// Finance Stubs
export interface PayoutDto {
  id: string;
  printerOwnerUserId: string;
  amount: number;
  status: PayoutStatus;
  createdAt: string;
  sentAt?: string;
}

export interface PlatformRevenueDto {
  totalRevenueEgp: number;
  payoutsTotalEgp: number;
  platformMarginEgp: number;
}
