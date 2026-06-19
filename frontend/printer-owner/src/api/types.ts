// Printer-owner API DTOs. Mirrors the shapes returned by the ASP.NET controllers.
// Where a backend controller does not exist yet, the type reflects the planned contract.

export interface AuthResultDto {
  accessToken: string;
  refreshToken: string;
  userId: string;
  fullName: string;
  role: string;
}

export interface RegisterPrinterOwnerCommand {
  fullNameAr: string;
  fullNameEn: string;
  phoneNumber: string;
  email: string;
  password: string;
}

export interface LoginCommand {
  emailOrPhone: string;
  password: string;
}

export interface VerifyPhoneOtpCommand {
  phoneNumber: string;
  otp: string;
}

export interface CompleteOnboardingCommand {
  businessName: string;
  nationalId: string;
  instapayNumber: string;
  district: string;
  city: string;
  idleHoursPerDay: number;
  agreementAccepted: boolean;
}

export type PrinterTechnology = 'FDM' | 'MSLA_Resin' | 'SLA' | 'SLS';
export type PrinterStatus = 'PendingVerification' | 'Active' | 'Paused' | 'Decommissioned';

export interface PrinterDto {
  id: string;
  brand: string;
  model: string;
  technologyType: PrinterTechnology;
  buildVolumeX: number;
  buildVolumeY: number;
  buildVolumeZ: number;
  areaDistrict: string;
  status: PrinterStatus;
  certificationLevel: string;
}

export interface AddPrinterCommand {
  brand: string;
  model: string;
  technologyType: PrinterTechnology;
  buildVolumeX: number;
  buildVolumeY: number;
  buildVolumeZ: number;
  areaDistrict: string;
  areaCity: string;
  supportedMaterials: string[];
}

export type JobStatus =
  | 'Offered' | 'Accepted' | 'Rejected' | 'Printing' | 'QCPending'
  | 'QCApproved' | 'QCRejected' | 'Ready' | 'Shipped' | 'Delivered' | 'Failed';

export interface JobAssignmentDto {
  id: string;
  orderItemId: string;
  printerId: string;
  status: JobStatus;
  payoutAmount: number;
  offeredAt: string;
  acceptanceDeadline: string;
  slicerFileStorageKey?: string;
}

export interface OwnerEarningsDto {
  thisWeekEgp: number;
  thisMonthEgp: number;
  allTimeEgp: number;
  nextPayoutDate?: string;
  nextPayoutEstimateEgp: number;
}

export interface EarningsBreakdownItem {
  jobAssignmentId: string;
  date: string;
  payoutAmountEgp: number;
  qcResult: 'Passed' | 'Failed';
}

// Gamification read model — no controller yet (TODO(backend)).
export interface AchievementsDto {
  level: number;
  levelName: string;
  totalXp: number;
  nextLevelXp: number;
  badges: { id: string; name: string; unlocked: boolean }[];
  currentStreak: number;
}
