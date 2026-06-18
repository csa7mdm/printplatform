export const meta = {
  name: 'egypt-3d-print-backend',
  description: 'Backend domain modules + wiring for the Egypt 3D Print Platform (scaffold + packages already on disk)',
  phases: [
    { title: 'Domain Core', detail: 'Identity, Marketplace, Orders in parallel' },
    { title: 'Domain Advanced', detail: 'Dispatch, Finance/Ledger, Integrations in parallel' },
    { title: 'Wire & Verify', detail: 'Event wiring, migrations, integration tests' },
  ],
};

const RESULT_SCHEMA = {
  type: 'object',
  required: ['filesCreated', 'summary', 'openItems'],
  properties: {
    filesCreated: { type: 'array', items: { type: 'string' } },
    summary:      { type: 'string' },
    openItems:    { type: 'array', items: { type: 'string' } },
  },
};

const PREAMBLE = `
CONTEXT: The solution scaffold AND the two reusable packages (PrintPlatform.Gamification,
PrintPlatform.Loyalty) ALREADY EXIST on disk at E:\\workspace\\3D Printing SAAS\\src\\.
Do NOT recreate them. Read the existing scaffold (base types: IAggregateRoot, BaseEntity<TId>,
Result<T>, IDomainEventDispatcher in PrintPlatform.Domain) and build ON TOP of it.
Stack is .NET 10 / ASP.NET Core 10, EF Core 10.0.9, MediatR, Hangfire. OpenAPI via
Microsoft.AspNetCore.OpenApi + Scalar (NOT Swashbuckle). Resilience via
Microsoft.Extensions.Http.Resilience (NOT Polly.Extensions.Http).
`;

// ─── PHASE 1 — DOMAIN CORE ──────────────────────────────────────────────────
phase('Domain Core');

const [identity, marketplace, orders] = await parallel([

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement the IDENTITY MODULE for the Egypt 3D Print Platform.
TWO-SIDED MARKETPLACE: Customers who order, Printer Owners who fulfill.

DOMAIN (PrintPlatform.Domain/Identity/): User (Guid Id, Email, PhoneNumber, FullNameAr,
FullNameEn, UserRole{Customer,PrinterOwner,Operator,Admin}, Lang{Arabic,English}, IsActive,
IsVerified, soft-delete, RefreshTokens, RowVersion; events UserRegisteredEvent, UserVerifiedEvent);
RefreshToken value object (hashed Token, ExpiresAt, CreatedByIp, RevokedAt, ReplacedByToken);
CustomerProfile (UserId 1:1, DefaultAddressId, LoyaltyMemberId string, PreferredPaymentMethod);
PrinterOwnerProfile (UserId 1:1, NationalId encrypted, BusinessName, BankAccountDetails encrypted,
InstapayNumber, CertificationLevel{Pending,Basic,Certified,Expert,Elite}, GamificationPlayerId string,
IsAvailable, OnboardingCompletedAt, CertificationScore decimal; events PrinterOwnerCertifiedEvent,
CertificationLevelChangedEvent).

APPLICATION (PrintPlatform.Application/Identity/): Commands RegisterCustomer, RegisterPrinterOwner,
Login(->Access+Refresh token), RefreshToken, RevokeToken, VerifyPhoneOtp, CompletePrinterOwnerOnboarding;
Queries GetCurrentUser, GetPrinterOwnerProfile. All handlers return Result<T> (never throw for business logic).

INFRASTRUCTURE (PrintPlatform.Infrastructure/Identity/): ASP.NET Identity ApplicationUser:IdentityUser<Guid>;
JWT HS256 access 15min, refresh 30d stored in DB + HttpOnly cookie; IOtpSender + stub WhatsAppOtpSender;
NationalId & BankAccountDetails encrypted via ASP.NET Data Protection.

API (PrintPlatform.API/Controllers/): AuthController POST register/customer, register/printer-owner,
login, refresh, revoke, verify-otp; ProfileController GET /api/profile/me, PUT /api/profile/printer-owner/onboarding.

Soft delete global filter, optimistic concurrency via RowVersion. Write full implementations, not stubs.
Output every file path created + summary.
`, { label: 'domain:identity', phase: 'Domain Core', schema: RESULT_SCHEMA }),

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement the MARKETPLACE MODULE (supply side).

DOMAIN (PrintPlatform.Domain/Marketplace/): Printer (Guid Id, PrinterOwnerProfileId, Brand, Model,
TechnologyType{FDM,MSLA_Resin,SLA,SLS}, BuildVolumeX/Y/Z, NozzleDiameter?, SupportedMaterials JSON,
Max/MinLayerHeight, AreaDistrict, AreaCity, IsDeliverable, Status{PendingVerification,Active,Paused,
Decommissioned}, CertificationLevel, soft-delete); MaterialOption (Guid Id, MaterialType{PLA,PETG,ABS,
TPU,Nylon,PLA_CF,Resin_Standard,Resin_Dental,Resin_Engineering}, ColorName, ColorHex, PricePerGram,
OwnerCostPerGram, Properties JSON flags, IsActive); PrinterMaterial join (PrinterId, MaterialOptionId,
MaxQuality{Draft,Standard,Fine,Ultra}, IsDefault).

APPLICATION (PrintPlatform.Application/Marketplace/): Commands AddPrinter (status=PendingVerification),
UpdatePrinterAvailability, VerifyPrinter, UpdateCertificationScore; Queries GetMyPrinters,
GetAvailablePrintersForJob (RANKED): score = capabilityMatch*40 + proximityScore*30 +
certificationScore*20 + loadScore*10; capabilityMatch hard filter (material+build volume) 1/0;
proximityScore = 1-(distanceKm/30) clamp 0..1; certificationScore = level/5; loadScore =
1-(activeJobs/maxConcurrent).

INFRASTRUCTURE: EF config; DistrictDistanceTable.cs hardcoded Cairo/Giza district pairs approx km (no PostGIS).

API: PrintersController GET /api/printers/mine, POST /api/printers, PUT /api/printers/{id}/availability,
POST /api/printers/{id}/verify, GET /api/printers/available-for-job/{jobId}; MaterialsController GET
/api/materials, GET /api/materials/{id}. Handlers return Result<T>. Full implementations. Output files + summary.
`, { label: 'domain:marketplace', phase: 'Domain Core', schema: RESULT_SCHEMA }),

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement the ORDERS + QUOTING MODULE (demand side).

DOMAIN (PrintPlatform.Domain/Orders/): ModelFile (Guid Id, CustomerUserId, StorageKey [S3/MinIO key,
never binary in DB], OriginalFileName, FileSizeBytes, FileFormat{STL,ThreeMF,OBJ}, Status{Uploaded,
AnalysisPending,Approved,Rejected}, EstimatedVolumeCC, BoundingBoxX/Y/Z, EstimatedWeightGrams,
EstimatedPrintHours, AnalysisNotes); QuoteRequest (Guid Id, ModelFileId, CustomerUserId, MaterialOptionId,
QualityPreset{Draft,Standard,Fine,Ultra}, LayerHeightMm, InfillPercent, Quantity, IncludeDesignService,
Status{PendingReview,Quoted,Expired,Accepted,Rejected}); Quote (Guid Id, QuoteRequestId 1:1,
EstimatedWeightGrams, EstimatedPrintHours, MaterialCostEgp, DepreciationCostEgp, LaborCostEgp,
PackagingCostEgp, FailureAllowanceEgp, TotalFulfillmentCostEgp, PlatformMarginEgp, CustomerPriceEgp,
DesignServicePriceEgp, DeliveryEstimateEgp, TotalPriceEgp, EstimatedDeliveryDate, ValidUntil,
OperatorNotes, Status{Draft,Sent,Accepted,Expired}); Order aggregate (Guid Id, CustomerUserId,
Status{PendingPayment,Confirmed,InProduction,QCPending,ReadyToShip,Shipped,Delivered,Cancelled,Refunded},
PaymentMethod{COD,InstaPay,VodafoneCash,Card,BankTransfer}, PaymentStatus{Pending,Paid,PartiallyPaid,
Refunded}, PaymobOrderId?, ShippingAddressSnapshot JSON, SubtotalEgp, DeliveryFeeEgp, DiscountEgp,
TotalEgp, LoyaltyPointsUsed, LoyaltyDiscountEgp, RowVersion; events OrderConfirmedEvent,
OrderCancelledEvent, OrderDeliveredEvent); OrderItem (Guid Id, OrderId, QuoteId, Quantity, UnitPriceEgp,
TotalEgp, ModelFileStorageKey snapshot).

APPLICATION (PrintPlatform.Application/Orders/): Commands UploadModelFile (->MinIO via IFileStorageService,
publish ModelFileUploadedEvent), CreateQuoteRequest, ConfirmQuote (operator final Quote), AcceptQuote
(creates Order PendingPayment), InitiatePayment (Paymob, returns iframe URL/payment key), ConfirmPayment
(Paymob webhook -> Confirmed), CancelOrder; Queries GetMyOrders, GetOrderDetails, GetPendingQuotes.

BACKGROUND (Hangfire): ModelGeometryAnalysisJob on ModelFileUploadedEvent -> download STL from MinIO ->
implement BinaryStlReader, compute volume via triangle mesh sum -> weight=volume*density -> printHours=
weight/speed -> update ModelFile -> publish ModelFileAnalysedEvent.

API: ModelsController POST /api/models/upload (multipart, max 50MB), GET /api/models/{id}; QuotesController
POST /api/quotes/request, PUT /api/quotes/{id}/confirm, POST /api/quotes/{id}/accept; OrdersController GET
/api/orders, GET /api/orders/{id}, POST /api/orders/{id}/payment/initiate, POST /api/orders/{id}/cancel,
POST /api/payments/webhook ([AllowAnonymous], Paymob HMAC verified).

PAYMOB (PrintPlatform.Infrastructure/Payments/): IPaymentGateway + PaymobGateway making real Paymob v2 HTTP
calls (auth token -> order registration -> payment key -> iframe URL); HmacValidator for webhook signature.
Handlers return Result<T>. Full implementations. Output files + summary.
`, { label: 'domain:orders', phase: 'Domain Core', schema: RESULT_SCHEMA }),

]);

log(`Domain Core — Identity:${identity?.filesCreated?.length ?? 'FAILED'} Marketplace:${marketplace?.filesCreated?.length ?? 'FAILED'} Orders:${orders?.filesCreated?.length ?? 'FAILED'}`);

// ─── PHASE 2 — DOMAIN ADVANCED ──────────────────────────────────────────────
phase('Domain Advanced');

const [dispatch, finance, integrations] = await parallel([

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement the DISPATCH MODULE (routes jobs to owners + QC).

DOMAIN (PrintPlatform.Domain/Dispatch/): JobAssignment aggregate (Guid Id, OrderItemId, PrinterId,
PrinterOwnerUserId, Status{Offered,Accepted,Rejected,Printing,QCPending,QCApproved,QCRejected,Ready,
Shipped,Delivered,Failed}, OfferedAt, AcceptedAt, PrintingStartedAt, CompletedAt, AcceptanceDeadline
(=OfferedAt+3h), PayoutAmount, PayoutStatus{Pending,Batched,Paid}, SlicerFileStorageKey, OperatorNotes,
RowVersion; events JobOfferedEvent, JobAcceptedEvent, JobRejectedEvent, QCApprovedEvent, QCRejectedEvent,
JobDeliveredEvent); QCRecord (Guid Id, JobAssignmentId, PhotoStorageKeys JSON, OperatorDecision{Approved,
Rejected}, Notes, ChecklistJson, OperatorUserId).

APPLICATION (PrintPlatform.Application/Dispatch/): AssignJob (creates Offered, WhatsApp notify owner,
schedule AcceptanceDeadlineCheckJob in 3h, raise JobOfferedEvent), AcceptJob, RejectJob (auto re-route to
next ranked printer), StartPrinting, UploadCompletionPhotos (->QCPending), ApproveQC (QCRecord Approved ->
QCApproved -> Bosta CreateShipment -> raise QCApprovedEvent), RejectQC (QCRecord Rejected -> reprint or
re-route, notify customer), ConfirmDelivery (Bosta webhook -> Delivered, raise JobDeliveredEvent).

BACKGROUND: AcceptanceDeadlineCheckJob -> if still Offered past deadline, auto-reject + re-offer next.

BOSTA (PrintPlatform.Infrastructure/Shipping/): IShippingService + BostaShippingService (CreateShipment ->
trackingNumber+bostaOrderId, GetShipmentStatus, CancelShipment); POST /api/shipping/webhook HMAC verify.

API: DispatchController GET /api/dispatch/pending, POST /api/dispatch/assign, POST /api/dispatch/{id}/accept,
/reject, /start, /complete (multipart photos), POST /api/dispatch/{id}/qc/approve, /qc/reject, GET
/api/dispatch/my-jobs, POST /api/shipping/webhook ([AllowAnonymous]+HMAC). Handlers return Result<T>.
Optimistic concurrency via RowVersion so two operators can't dispatch same job. Full implementations.
Output files + summary.
`, { label: 'domain:dispatch', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement the FINANCE MODULE (immutable ledger + payouts).

DOMAIN (PrintPlatform.Domain/Finance/): LedgerEntry APPEND-ONLY (Guid Id, EntryType{CustomerPayment,
PlatformCommission,OwnerPayoutBatched,OwnerPayoutPaid,Refund,AdjustmentCredit,AdjustmentDebit}, AmountEgp,
Direction{In,Out}, RelatedOrderId?, RelatedJobAssignmentId?, RelatedPayoutId?, PrinterOwnerUserId?,
CustomerUserId?, Description, ExternalReference, CreatedAt set once, CreatedByUserId); Payout (Guid Id,
PrinterOwnerUserId, PeriodStart, PeriodEnd, GrossAmountEgp, PlatformFeeEgp, NetAmountEgp, Status{Calculating,
ReadyToSend,Sent,Failed,Reconciled}, PaymentMethod{InstaPay,BankTransfer}, ExternalReference,
JobAssignmentIds JSON, CreatedAt, SentAt, ReconciledAt).

APPLICATION (PrintPlatform.Application/Finance/): RecordCustomerPayment (LedgerEntry CustomerPayment In +
PlatformCommission In derived), InitiateWeeklyPayouts (group unpaid JobAssignments by owner, net=AOV-
commission, Payout ReadyToSend, LedgerEntry OwnerPayoutBatched), MarkPayoutSent (Sent + ExternalReference +
LedgerEntry OwnerPayoutPaid), RecordRefund (LedgerEntry Refund Out); Queries GetOwnerEarnings,
GetPlatformRevenue, GetPendingPayouts, GetLedgerAudit.

API: FinanceController GET /api/finance/my-earnings, /my-earnings/breakdown, /payouts, POST
/api/finance/payouts/{id}/mark-sent, GET /api/finance/platform/revenue, /ledger.

BACKGROUND: WeeklyPayoutBatchJob (cron "0 0 * * 0") -> InitiateWeeklyPayouts for all eligible owners +
WhatsApp notify each owner + admin summary. CRITICAL: every LedgerEntry write is in a DB transaction with
the triggering entity update (IDbContextTransaction, no partial states). Handlers return Result<T>.
Full implementations. Output files + summary.
`, { label: 'domain:finance', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

  () => agent(PREAMBLE + `
You are a senior .NET engineer. Implement ALL EXTERNAL INTEGRATIONS in
PrintPlatform.Infrastructure as concrete adapters behind interfaces declared in PrintPlatform.Domain.

1. WHATSAPP: INotificationService (Domain) + WhatsAppNotificationService (Infrastructure/Notifications/)
using Meta Cloud API graph.facebook.com/v18.0/{phone_number_id}/messages, Bearer token from config.
Methods: SendQuoteReady, SendOrderConfirmed, SendJobOffered, SendQCRejected, SendQCPhotosForApproval,
SendShipped, SendPayoutProcessed, SendOtp, SendAchievementUnlocked, SendLevelUp, SendTierUpgrade.
Fire-and-forget with retry via Microsoft.Extensions.Http.Resilience (3 retries, exponential backoff).
Log sends to Serilog (template name + recipient, no PII).

2. FILE STORAGE: IFileStorageService (Domain) + MinioFileStorageService (Infrastructure/Storage/) using
AWSSDK.S3. Methods: Upload(stream,key,contentType,metadata)->key, GetPresignedDownloadUrl(key,60),
GetPresignedUploadUrl(key,30), Delete, Exists. Keys: models/{userId}/{orderId}/{file}.stl,
qc-photos/{jobAssignmentId}/{ts}_{i}.jpg, slicer-files/{jobAssignmentId}/print_file.gcode. StorageOptions.

3. BACKGROUND JOB REGISTRATION: Infrastructure/BackgroundJobs/HangfireJobRegistrar.cs registering
RecurringJob weekly-payouts "0 0 * * 0", acceptance-check "*/5 * * * *", loyalty-expiry "0 2 * * *".

4. WHATSAPP WEBHOOK: POST /api/webhooks/whatsapp [AllowAnonymous] (verify hub.challenge on GET, verify
X-Hub-Signature-256 on POST, parse incoming messages, route via IMediator).

5. HEALTH CHECKS: /health/live basic; /health/ready PostgreSQL + MinIO + WhatsApp reachability.

6. SERILOG: Console (JSON prod / pretty dev), File (rolling daily 30d), Enrich correlation id + Application
property, filter health-check noise, log HTTP requests (StatusCode, ElapsedMs, UserId).

7. GLOBAL EXCEPTION HANDLER: IExceptionHandler (ASP.NET Core 10) -> DomainException 400 ProblemDetails,
NotFoundException 404, ConflictException 409, Unauthorized 401, unhandled 500 (log w/ correlation id, no
stack leak). Always application/problem+json. Full implementations. Output files + summary.
`, { label: 'infra:integrations', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

]);

log(`Domain Advanced — Dispatch:${dispatch?.filesCreated?.length ?? 'FAILED'} Finance:${finance?.filesCreated?.length ?? 'FAILED'} Integrations:${integrations?.filesCreated?.length ?? 'FAILED'}`);

// ─── PHASE 3 — WIRE & VERIFY ────────────────────────────────────────────────
phase('Wire & Verify');

const wiring = await agent(PREAMBLE + `
You are a senior .NET engineer. Wire all the pieces together and write integration tests.
All domain modules (Identity, Marketplace, Orders, Dispatch, Finance, Integrations) and both packages
(Gamification, Loyalty) now exist on disk.

1. DOMAIN EVENT WIRING (PrintPlatform.Application/EventHandlers/):
GamificationEventHandler: JobAcceptedEvent->AwardPoints(owner,"JobAccepted",5);
QCApprovedEvent->AwardPoints(owner,"QCApproved",20); QCRejectedEvent->ResetStreak(owner,"QCApproval");
JobDeliveredEvent->AwardPoints(owner,"OrderDelivered",10); ReviewReceivedEvent->AwardPoints(owner,
"ReviewReceived",5). LoyaltyEventHandler: OrderConfirmedEvent->Earn(customer,order.TotalEgp,orderId);
ReviewLeftEvent->Earn(customer,0,orderId,"ReviewBonus") fixed 50pts. GamificationNotificationHandler:
AchievementUnlockedEvent->whatsapp.SendAchievementUnlocked; LevelUpEvent->whatsapp.SendLevelUp.
LoyaltyNotificationHandler: TierUpgradedEvent->whatsapp.SendTierUpgrade.

2. DBCONTEXT (PrintPlatform.Infrastructure/Data/AppDbContext.cs): register all entity configs, global
query filter !IsDeleted on ISoftDelete entities, RowVersion concurrency token on Order + JobAssignment,
enum->string value converters, JSON columns for List<string>/List<Guid>. Gamification & Loyalty keep
their OWN DbContexts (gam_/loy_ prefixes), same Postgres server: AddDbContext<GamificationDbContext> and
AddDbContext<LoyaltyDbContext> with UseNpgsql. Register AddGamification() + AddLoyalty() in Program.cs.

3. EF MIGRATIONS: create InitialCreate for AppDbContext, GamificationInit for GamificationDbContext,
LoyaltyInit for LoyaltyDbContext. DatabaseSeeder.cs seeds MaterialOption rows (all types + common colors),
Gamification Level rows, LoyaltyTier rows, Admin user (from config, not hardcoded).

4. INTEGRATION TESTS (PrintPlatform.Application.Tests/Integration/) using WebApplicationFactory<Program> +
Testcontainers PostgreSQL: OrderFlowIntegrationTest (register customer -> upload model -> request quote ->
operator confirm -> accept -> payment webhook -> assign -> owner accept -> photos -> QC approve -> Bosta
webhook -> delivered; assert LedgerEntry count=2 and Order.Status=Delivered); GamificationFlowIntegrationTest
(QCApproved -> +20 XP; 10 consecutive -> Perfectionist achievement); LoyaltyFlowIntegrationTest
(OrderConfirmed 1200 EGP -> 120 pts; accumulate 1000 -> Silver).

5. OPENAPI: XML doc comments on controllers, Scalar UI with JWT bearer input, export openapi.json to
E:\\workspace\\3D Printing SAAS\\docs\\openapi.json.

Make the solution build. Full implementations. Output files + summary.
`, { label: 'wire:events-tests', phase: 'Wire & Verify', schema: RESULT_SCHEMA });

const all = [identity, marketplace, orders, dispatch, finance, integrations, wiring].filter(Boolean);
const totalFiles = all.reduce((s, r) => s + (r?.filesCreated?.length ?? 0), 0);
const openItems = all.flatMap(r => r?.openItems ?? []);
log(`Backend complete — ${totalFiles} files, ${openItems.length} open items`);
return { totalFiles, openItems, summary: 'Backend domain modules + wiring built on existing scaffold + packages' };
