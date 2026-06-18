export const meta = {
  name: 'egypt-3d-print-platform-implementation',
  description: 'Build the Egypt 3D Print Marketplace — modular monolith + Gamification + Loyalty packages',
  phases: [
    { title: 'Scaffold', detail: 'Solution structure, shared contracts, DB, CI/CD skeleton' },
    { title: 'Packages', detail: 'Gamification and Loyalty engines — zero domain coupling' },
    { title: 'Domain Core', detail: 'Identity, Marketplace, Orders in parallel' },
    { title: 'Domain Advanced', detail: 'Dispatch, Finance/Ledger, Integrations in parallel' },
    { title: 'Frontend', detail: 'Customer, Operator, Printer Owner apps in parallel' },
    { title: 'Wire & Verify', detail: 'Event wiring, end-to-end tests, Docker, CI/CD' },
  ],
};

// ─── SHARED SCHEMA ──────────────────────────────────────────────────────────
const RESULT_SCHEMA = {
  type: 'object',
  required: ['filesCreated', 'summary', 'openItems'],
  properties: {
    filesCreated: { type: 'array', items: { type: 'string' } },
    summary:      { type: 'string' },
    openItems:    { type: 'array', items: { type: 'string' } },
  },
};

// ─── PHASE 1 — SCAFFOLD (everything else depends on this) ───────────────────
phase('Scaffold');

const scaffold = await agent(`
You are a senior .NET architect. Create the solution scaffold for a production-ready
ASP.NET Core 10 modular monolith called "PrintPlatform".

SOLUTION STRUCTURE to create under E:\\workspace\\3D Printing SAAS\\src\\:

PrintPlatform.sln
src/
  PrintPlatform.API/                  ← ASP.NET Core 10 Web API entry point
  PrintPlatform.Domain/               ← Domain entities, events, interfaces
  PrintPlatform.Application/          ← Use cases, command/query handlers (MediatR)
  PrintPlatform.Infrastructure/       ← EF Core, external adapters
  packages/
    PrintPlatform.Gamification/       ← Reusable package (no domain imports)
    PrintPlatform.Loyalty/            ← Reusable package (no domain imports)
tests/
  PrintPlatform.Domain.Tests/
  PrintPlatform.Application.Tests/
  PrintPlatform.Gamification.Tests/
  PrintPlatform.Loyalty.Tests/

TASKS:
1. Create the .sln file referencing all projects
2. Create each .csproj with correct NuGet packages:
   - API: Serilog.AspNetCore 9.0.0, Microsoft.AspNetCore.OpenApi 10.0.9,
          Scalar.AspNetCore 2.16.4, JwtBearer 10.0.9, Hangfire.AspNetCore 1.8.14
   - Domain: none (pure C#)
   - Application: MediatR 12.4.1, FluentValidation 11.11.0, Mapster 7.4.0
   - Infrastructure: EF Core 10.0.9, Npgsql.EF 10.0.2, Identity.EF 10.0.9,
          AWSSDK.S3 3.7.x, Polly 8.5.0, Microsoft.Extensions.Http.Resilience 10.7.0,
          DataProtection 10.0.9, Hangfire.Core 1.8.14
   - Gamification: EF Core 10.0.9, Npgsql.EF 10.0.2, MediatR 12.4.1,
          Microsoft.Extensions.Options 10.0.9 (no API or Domain project refs)
   - Loyalty: same as Gamification (no API or Domain project refs)
3. Create Program.cs with full DI registration skeleton:
   - AddGamification() + AddLoyalty() extension call placeholders
   - AddHangfire() with PostgreSQL storage
   - Serilog configuration
   - Swagger/OpenAPI with JWT bearer support
   - CORS for frontend origins
4. Create appsettings.json with all config sections:
   ConnectionStrings: { DefaultConnection, HangfireConnection }
   Jwt: { Secret, Issuer, Audience, AccessTokenMinutes, RefreshTokenDays }
   Storage: { Provider, BucketName, Endpoint }
   Paymob: { ApiKey, IntegrationId, IframeId }
   Bosta: { ApiKey, BaseUrl }
   WhatsApp: { Token, PhoneNumberId }
   Gamification: { } (placeholder)
   Loyalty: { } (placeholder)
5. Create base domain types in PrintPlatform.Domain:
   - IAggregateRoot, IEntity<TId>, IDomainEvent interfaces
   - BaseEntity<TId> with CreatedAt, UpdatedAt, IsDeleted (soft delete)
   - IDomainEventDispatcher interface
   - Result<T> and Error value types (no exceptions for business logic)
6. Create shared InternalEventBus using MediatR INotificationHandler pattern
7. Create Dockerfile (multi-stage: build → publish → runtime on mcr.microsoft.com/dotnet/aspnet:10.0)
8. Create docker-compose.yml with: api, postgres, minio services
9. Create .github/workflows/ci.yml: restore → build → test → docker build on push

Output: list every file path created and a brief summary.
`, { label: 'scaffold:solution', phase: 'Scaffold', schema: RESULT_SCHEMA });

log(`Scaffold complete: ${scaffold.filesCreated.length} files created`);

// ─── PHASE 2 — PACKAGES (parallel, depend only on scaffold) ─────────────────
phase('Packages');

const [gamification, loyalty] = await parallel([

  () => agent(`
You are a senior .NET engineer. Build the COMPLETE Gamification Engine package at:
E:\\workspace\\3D Printing SAAS\\src\\packages\\PrintPlatform.Gamification\\

DESIGN CONTRACT (non-negotiable):
- NEVER import PrintPlatform.Domain, .Application, or .API namespaces
- Operates only on string ExternalUserId — no domain User entity
- Own EF Core entity configuration via IEntityTypeConfiguration<T>
- Registered via IServiceCollection.AddGamification(Action<GamificationOptions>)
- Configured via IOptions<GamificationOptions>
- Raises its own events out (INotification), consumed by the host platform

FOLDER STRUCTURE:
Abstractions/
  IGamificationService.cs     ← main service interface the host calls
  IAchievementRule.cs         ← pluggable rule interface
  ILeaderboardProvider.cs
Models/
  GamificationPlayer.cs       ← ExternalUserId, DisplayName, TotalXP, CurrentLevel, Streaks
  PointTransaction.cs         ← PlayerId, Points, Action, Metadata(JSON), CreatedAt [append-only]
  Achievement.cs              ← Id, Name, Description, IconSlug, CriteriaJson, Category
  PlayerAchievement.cs        ← PlayerId, AchievementId, UnlockedAt
  Level.cs                    ← Tier(int), Name, MinXP, MaxXP, BenefitsJson
  Challenge.cs                ← Id, Name, CriteriaJson, RewardXP, StartDate, EndDate
  PlayerChallenge.cs          ← PlayerId, ChallengeId, Progress, CompletedAt
  Streak.cs                   ← PlayerId, ActionType, CurrentCount, LastActionDate
  Leaderboard.cs              ← Id, Name, Period(Daily/Weekly/AllTime), Type(XP/JobCount/Rating)
  LeaderboardEntry.cs         ← LeaderboardId, PlayerId, Score, Rank, UpdatedAt
Services/
  GamificationService.cs      ← implements IGamificationService
  AchievementEngine.cs        ← evaluates IAchievementRule[] after every point award
  StreakService.cs             ← maintains streaks, resets on gap
  LeaderboardService.cs       ← upserts entries, computes rank
Rules/
  ConsecutiveQCApprovalRule.cs  ← "Perfectionist" badge: 10 QCApproved in a row
  MilestoneJobRule.cs           ← badges at 1, 10, 50, 100, 500 jobs
  SpeedDeliveryRule.cs          ← "Speed Demon": delivered within 24h
  ProfileCompleteRule.cs        ← awards XP when all profile fields filled
Data/
  GamificationDbContext.cs    ← separate DbContext, all tables prefixed gam_
  Configurations/             ← IEntityTypeConfiguration per entity
Events/
  PointsAwardedEvent.cs       ← INotification: UserId, Points, Action, NewTotal
  AchievementUnlockedEvent.cs ← INotification: UserId, AchievementId, AchievementName
  LevelUpEvent.cs             ← INotification: UserId, OldLevel, NewLevel
  StreakMilestoneEvent.cs     ← INotification: UserId, ActionType, Count
Extensions/
  ServiceCollectionExtensions.cs

LEVEL CONFIG for 3D printing platform (hardcode as default, overridable):
Level 1 — Novice       — 0 XP    — listed in network
Level 2 — Certified    — 200 XP  — priority in dispatch queue
Level 3 — Expert       — 700 XP  — 1% fee reduction
Level 4 — Master       — 2000 XP — featured on platform
Level 5 — Elite        — 5000 XP — white-glove support + custom SLA badge

ACTION → XP MAP (default, overridable via GamificationOptions):
JobAccepted=5, QCApproved=20, OrderDelivered=10, ProfileCompleted=50,
FirstJobCompleted=100, ReviewReceived=5, ChallengeCompleted=varies

Implement AwardPointsAsync to:
1. Insert PointTransaction (append-only, never update/delete)
2. Recalculate TotalXP on GamificationPlayer
3. Check if level threshold crossed → raise LevelUpEvent
4. Run AchievementEngine → for each newly satisfied rule → raise AchievementUnlockedEvent
5. Update relevant Streak rows
6. Update LeaderboardEntry scores

Write full implementations (not stubs). Include XML doc on all public interfaces.
Include GamificationOptions with sensible 3D-printing defaults.
Write unit tests in PrintPlatform.Gamification.Tests covering:
- AwardPointsAsync triggers level-up at threshold
- ConsecutiveQCApprovalRule fires at count=10, resets on QCRejected
- Leaderboard rank recalculates correctly
`, { label: 'package:gamification', phase: 'Packages', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior .NET engineer. Build the COMPLETE Loyalty Engine package at:
E:\\workspace\\3D Printing SAAS\\src\\packages\\PrintPlatform.Loyalty\\

DESIGN CONTRACT (non-negotiable):
- NEVER import PrintPlatform.Domain, .Application, or .API namespaces
- Operates only on string ExternalUserId — no domain User entity
- Own EF Core entity configuration, tables prefixed loy_
- Registered via IServiceCollection.AddLoyalty(Action<LoyaltyOptions>)
- Raises its own INotification events out
- Points ledger is APPEND-ONLY (no updates, no deletes)

FOLDER STRUCTURE:
Abstractions/
  ILoyaltyService.cs
  ITierStrategy.cs            ← pluggable tier evaluation
  IRewardProvider.cs
Models/
  LoyaltyMember.cs            ← ExternalUserId, CurrentTierId, LifetimePoints, ActivePoints
  PointsLedger.cs             ← MemberId, Delta(+/-), Type(Earned/Redeemed/Expired/Bonus),
                                 OrderReference, Expiry, CreatedAt [append-only]
  LoyaltyTier.cs              ← Id, Name, MinPoints, Multiplier, BenefitsJson, SortOrder
  Reward.cs                   ← Id, Name, Description, PointsCost, Type(Discount/FreeService/Credit), Value
  Redemption.cs               ← MemberId, RewardId, PointsUsed, OrderReference, CreatedAt
  Referral.cs                 ← ReferrerId, ReferredUserId, Code, Status, BonusPoints, CreatedAt
Services/
  LoyaltyService.cs           ← implements ILoyaltyService
  TierEvaluationService.cs    ← recalculates tier after every earn/redeem
  ReferralService.cs          ← generate codes, attribute conversions, award bonuses
  ExpiryService.cs            ← background: expire points older than config months
Data/
  LoyaltyDbContext.cs
  Configurations/
Events/
  PointsEarnedEvent.cs        ← INotification
  PointsRedeemedEvent.cs      ← INotification
  TierUpgradedEvent.cs        ← INotification: UserId, OldTier, NewTier
  TierDowngradedEvent.cs      ← INotification
  RewardRedeemedEvent.cs      ← INotification
  ReferralConvertedEvent.cs   ← INotification
Extensions/
  ServiceCollectionExtensions.cs

TIER CONFIG (default, overridable):
Bronze  — 0–999 pts     — 1.0× multiplier — standard
Silver  — 1000–4999 pts — 1.2× multiplier — 5% order discount, priority quote review
Gold    — 5000–14999 pts— 1.5× multiplier — 10% discount, 1 free design review/month
Platinum— 15000+ pts    — 2.0× multiplier — 15% discount, dedicated operator, SLA badge

EARNING RULES (default via LoyaltyOptions):
- 1 point per 10 EGP spent (PointsPerEgp = 0.1)
- Design service attach: 2× multiplier on that order
- Birthday month: 2× all earning (check DOB month on member)
- First order welcome: +200 bonus points
- Referral conversion: +500 to referrer after referee's first order

EarnAsync implementation must:
1. Look up or create LoyaltyMember
2. Apply tier multiplier to base points
3. Apply birthday multiplier if applicable
4. Insert PointsLedger row (append-only) with Expiry = now + ExpiryMonths
5. Recalculate ActivePoints (sum of non-expired ledger entries)
6. Evaluate tier via ITierStrategy → if changed, update CurrentTierId → raise TierUpgradedEvent
7. Raise PointsEarnedEvent

RedeemAsync implementation must:
1. Check ActivePoints >= reward.PointsCost
2. Insert negative PointsLedger row (Type=Redeemed)
3. Insert Redemption record
4. Raise RewardRedeemedEvent
5. Return RedemptionResult with discount value to apply to order

Write full implementations, not stubs. Include LoyaltyOptions with 3D-printing defaults.
Write unit tests covering:
- Tier upgrade triggers at exact threshold
- Expiry reduces ActivePoints correctly
- Referral bonus awarded only after referee's first order (not on signup)
- RedeemAsync fails gracefully if insufficient points
`, { label: 'package:loyalty', phase: 'Packages', schema: RESULT_SCHEMA }),

]);

log(`Packages: Gamification=${gamification?.filesCreated?.length ?? 'FAILED'} files, Loyalty=${loyalty?.filesCreated?.length ?? 'FAILED'} files`);

// ─── PHASE 3 — DOMAIN CORE (parallel, all depend on scaffold) ───────────────
phase('Domain Core');

const [identity, marketplace, orders] = await parallel([

  () => agent(`
You are a senior .NET engineer. Implement the IDENTITY MODULE for the Egypt 3D Print Platform.
Solution is at E:\\workspace\\3D Printing SAAS\\src\\

This is a TWO-SIDED MARKETPLACE: Customers who order prints, and Printer Owners who
fulfill them. This distinction drives all role logic.

IMPLEMENT in PrintPlatform.Domain + Application + Infrastructure:

DOMAIN ENTITIES (in PrintPlatform.Domain/Identity/):
User.cs
  - Id (Guid), Email, PhoneNumber, FullNameAr (Arabic), FullNameEn
  - Roles: enum UserRole { Customer, PrinterOwner, Operator, Admin }
  - PreferredLanguage: enum Lang { Arabic, English }
  - IsActive, IsVerified, CreatedAt, UpdatedAt, IsDeleted (soft delete)
  - RefreshTokens: List<RefreshToken> (value object)
  - Domain events: UserRegisteredEvent, UserVerifiedEvent

RefreshToken.cs (value object)
  - Token (hashed), ExpiresAt, CreatedByIp, RevokedAt, ReplacedByToken

CustomerProfile.cs
  - UserId (FK 1:1), DefaultAddressId, LoyaltyMemberId (string — links to Loyalty package)
  - PreferredPaymentMethod

PrinterOwnerProfile.cs
  - UserId (FK 1:1), NationalId (encrypted at rest), BusinessName
  - BankAccountDetails (encrypted), InstapayNumber
  - CertificationLevel: enum { Pending, Basic, Certified, Expert, Elite }
  - GamificationPlayerId (string — links to Gamification package)
  - IsAvailable, OnboardingCompletedAt
  - CertificationScore (decimal — computed from QC pass rate + ratings)
  - Domain events: PrinterOwnerCertifiedEvent, CertificationLevelChangedEvent

APPLICATION LAYER (PrintPlatform.Application/Identity/):
Commands:
  RegisterCustomerCommand / Handler
  RegisterPrinterOwnerCommand / Handler
  LoginCommand / Handler → returns AccessToken + RefreshToken
  RefreshTokenCommand / Handler
  RevokeTokenCommand / Handler
  VerifyPhoneOtpCommand / Handler
  CompletePrinterOwnerOnboardingCommand / Handler

Queries:
  GetCurrentUserQuery / Handler
  GetPrinterOwnerProfileQuery / Handler

INFRASTRUCTURE (PrintPlatform.Infrastructure/Identity/):
- ASP.NET Identity with custom ApplicationUser extending IdentityUser<Guid>
- JWT generation: RS256 or HS256, access token 15 min, refresh token 30 days
- Refresh tokens stored in DB, HttpOnly cookie delivery
- OTP via WhatsApp (stub WhatsAppOtpSender implementing IOtpSender)
- NationalId and BankAccountDetails encrypted using ASP.NET Data Protection

API CONTROLLERS (PrintPlatform.API/Controllers/):
AuthController:
  POST /api/auth/register/customer
  POST /api/auth/register/printer-owner
  POST /api/auth/login
  POST /api/auth/refresh
  POST /api/auth/revoke
  POST /api/auth/verify-otp
ProfileController:
  GET  /api/profile/me
  PUT  /api/profile/printer-owner/onboarding

IMPORTANT PATTERNS:
- All handlers return Result<T> (never throw for business logic)
- Optimistic concurrency: RowVersion on User entity
- Soft delete: IsDeleted filter applied globally in DbContext
- Arabic name stored separately for bilingual display
`, { label: 'domain:identity', phase: 'Domain Core', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior .NET engineer. Implement the MARKETPLACE MODULE — the supply side
of the Egypt 3D Print Platform (the "Uber driver" side).

Solution at E:\\workspace\\3D Printing SAAS\\src\\

CONTEXT: Printer owners join the platform with their printers. This is the core
supply-side moat — no equivalent exists in Egypt. Printer owners earn passive income
via InstaPay payouts. They must be verified and certified before receiving jobs.

IMPLEMENT in PrintPlatform.Domain + Application + Infrastructure:

DOMAIN ENTITIES (PrintPlatform.Domain/Marketplace/):
Printer.cs
  - Id (Guid), PrinterOwnerProfileId (FK)
  - Brand (string), Model (string), TechnologyType: enum { FDM, MSLA_Resin, SLA, SLS }
  - BuildVolumeX/Y/Z (mm, decimal)
  - NozzleDiameter (decimal, nullable — FDM only)
  - SupportedMaterials: List<MaterialType> (JSON column)
  - MaxLayerHeight / MinLayerHeight (decimal)
  - AreaDistrict (string), AreaCity (string), IsDeliverable (bool)
  - Status: enum { PendingVerification, Active, Paused, Decommissioned }
  - CertificationLevel (mirrors owner level)
  - CreatedAt, UpdatedAt, IsDeleted

MaterialOption.cs
  - Id (Guid), MaterialType: enum { PLA, PETG, ABS, TPU, Nylon, PLA_CF, Resin_Standard,
    Resin_Dental, Resin_Engineering }
  - ColorName (string), ColorHex (string)
  - PricePerGram (decimal) — what platform charges customer
  - OwnerCostPerGram (decimal) — owner's material cost reference
  - Properties: { flexible, food_safe, high_temp, biocompatible } (flags JSON)
  - IsActive

PrinterMaterial.cs (join: printer supports material at what quality)
  - PrinterId, MaterialOptionId, MaxQuality: enum { Draft, Standard, Fine, Ultra }
  - IsDefault

APPLICATION LAYER (PrintPlatform.Application/Marketplace/):
Commands:
  AddPrinterCommand / Handler — owner adds printer, status=PendingVerification
  UpdatePrinterAvailabilityCommand / Handler — toggle Active/Paused
  VerifyPrinterCommand / Handler — operator approves after physical check
  UpdateCertificationScoreCommand / Handler — recalculate from QC + rating history

Queries:
  GetMyPrintersQuery / Handler — printer owner sees their fleet
  GetAvailablePrintersForJobQuery / Handler — RANKED list for dispatch:
    Ranking formula:
      score = (capabilityMatch * 40) + (proximityScore * 30) + (certificationScore * 20) + (loadScore * 10)
    Where:
      capabilityMatch = 1 if printer supports required material + build volume, else 0 (hard filter)
      proximityScore  = 1 - (distanceToCustomerKm / 30).clamp(0,1)
      certificationScore = owner.CertificationLevel / 5
      loadScore = 1 - (activeJobCount / maxConcurrentJobs)

INFRASTRUCTURE:
- EF Core config for all entities
- Spatial distance: use simple district lookup table (no PostGIS needed for MVP)
  DistrictDistanceTable.cs — hardcoded Cairo/Giza district pairs with approx km

API CONTROLLERS (PrintPlatform.API/Controllers/):
PrintersController:
  GET    /api/printers/mine                    ← PrinterOwner only
  POST   /api/printers                         ← PrinterOwner only
  PUT    /api/printers/{id}/availability       ← PrinterOwner only
  POST   /api/printers/{id}/verify             ← Operator only
  GET    /api/printers/available-for-job/{jobId} ← Operator only, returns ranked list

MaterialsController:
  GET    /api/materials                        ← public
  GET    /api/materials/{id}                   ← public
`, { label: 'domain:marketplace', phase: 'Domain Core', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior .NET engineer. Implement the ORDERS + QUOTING MODULE for the
Egypt 3D Print Platform.

Solution at E:\\workspace\\3D Printing SAAS\\src\\

CONTEXT: Customer uploads a 3D model file, gets a rough instant estimate,
then an operator reviews and confirms a final quote. Customer accepts and pays
(or chooses COD). This is the demand-side core flow.

IMPLEMENT in PrintPlatform.Domain + Application + Infrastructure:

DOMAIN ENTITIES (PrintPlatform.Domain/Orders/):
ModelFile.cs
  - Id (Guid), CustomerUserId (Guid FK)
  - StorageKey (string — S3/MinIO object key, never store binary in DB)
  - OriginalFileName, FileSizeBytes, FileFormat: enum { STL, ThreeMF, OBJ }
  - Status: enum { Uploaded, AnalysisPending, Approved, Rejected }
  - EstimatedVolumeCC (decimal), BoundingBoxX/Y/Z (mm)
  - EstimatedWeightGrams (decimal), EstimatedPrintHours (decimal)
  - AnalysisNotes (string) — operator comments on printability
  - CreatedAt, UpdatedAt

QuoteRequest.cs
  - Id (Guid), ModelFileId (FK), CustomerUserId
  - MaterialOptionId (FK), QualityPreset: enum { Draft, Standard, Fine, Ultra }
  - LayerHeightMm (decimal), InfillPercent (int), Quantity (int)
  - IncludeDesignService (bool)
  - Status: enum { PendingReview, Quoted, Expired, Accepted, Rejected }
  - CreatedAt

Quote.cs
  - Id (Guid), QuoteRequestId (FK 1:1)
  - EstimatedWeightGrams, EstimatedPrintHours
  - MaterialCostEgp, DepreciationCostEgp, LaborCostEgp, PackagingCostEgp
  - FailureAllowanceEgp, TotalFulfillmentCostEgp
  - PlatformMarginEgp, CustomerPriceEgp
  - DesignServicePriceEgp (0 if not requested)
  - DeliveryEstimateEgp, TotalPriceEgp
  - EstimatedDeliveryDate, ValidUntil
  - OperatorNotes, Status: enum { Draft, Sent, Accepted, Expired }

Order.cs (aggregate root)
  - Id (Guid), CustomerUserId (Guid FK)
  - Status: enum { PendingPayment, Confirmed, InProduction, QCPending,
                   ReadyToShip, Shipped, Delivered, Cancelled, Refunded }
  - PaymentMethod: enum { COD, InstaPay, VodafoneCash, Card, BankTransfer }
  - PaymentStatus: enum { Pending, Paid, PartiallyPaid, Refunded }
  - PaymobOrderId (string, nullable)
  - ShippingAddressSnapshot (JSON value object — snapshot at order time)
  - SubtotalEgp, DeliveryFeeEgp, DiscountEgp, TotalEgp
  - LoyaltyPointsUsed (int), LoyaltyDiscountEgp (decimal)
  - RowVersion (byte[] — optimistic concurrency)
  - Domain events: OrderConfirmedEvent, OrderCancelledEvent, OrderDeliveredEvent

OrderItem.cs
  - Id (Guid), OrderId (FK), QuoteId (FK)
  - Quantity, UnitPriceEgp, TotalEgp
  - ModelFileStorageKey (snapshot)

APPLICATION LAYER (PrintPlatform.Application/Orders/):
Commands:
  UploadModelFileCommand / Handler
    → uploads to MinIO via IFileStorageService
    → triggers background geometry analysis (publish ModelFileUploadedEvent)
  CreateQuoteRequestCommand / Handler
  ConfirmQuoteCommand / Handler — operator sets final Quote
  AcceptQuoteCommand / Handler — customer accepts, creates Order (status=PendingPayment)
  InitiatePaymentCommand / Handler — calls Paymob, returns iframe URL or payment key
  ConfirmPaymentCommand / Handler — Paymob webhook → Order status=Confirmed
  CancelOrderCommand / Handler

Queries:
  GetMyOrdersQuery / Handler — customer sees their orders
  GetOrderDetailsQuery / Handler
  GetPendingQuotesQuery / Handler — operator queue

BACKGROUND HANDLER (Hangfire):
ModelGeometryAnalysisJob — triggered by ModelFileUploadedEvent
  → download STL from MinIO
  → compute volume via basic triangle mesh sum (implement BinaryStlReader)
  → estimate weight = volume * material_density
  → estimate print time = weight / print_speed_grams_per_hour
  → update ModelFile with estimates
  → publish ModelFileAnalysedEvent

API CONTROLLERS:
ModelsController:
  POST /api/models/upload          ← multipart, customer only, max 50MB
  GET  /api/models/{id}

QuotesController:
  POST /api/quotes/request         ← customer
  PUT  /api/quotes/{id}/confirm    ← operator
  POST /api/quotes/{id}/accept     ← customer

OrdersController:
  GET  /api/orders                 ← customer sees own; operator sees all
  GET  /api/orders/{id}
  POST /api/orders/{id}/payment/initiate
  POST /api/orders/{id}/cancel
  POST /api/payments/webhook       ← Paymob HMAC-verified webhook, [AllowAnonymous]

PAYMOB STUB (PrintPlatform.Infrastructure/Payments/):
  IPaymentGateway interface
  PaymobGateway implementing it — make real HTTP calls to Paymob API v2:
    1. Authentication request → get token
    2. Order registration
    3. Payment key request
    4. Return iframe URL to frontend
  HmacValidator — verify webhook signature
`, { label: 'domain:orders', phase: 'Domain Core', schema: RESULT_SCHEMA }),

]);

log(`Domain Core done — Identity: ${identity?.filesCreated?.length ?? 'FAILED'}, Marketplace: ${marketplace?.filesCreated?.length ?? 'FAILED'}, Orders: ${orders?.filesCreated?.length ?? 'FAILED'} files`);

// ─── PHASE 4 — DOMAIN ADVANCED (parallel) ───────────────────────────────────
phase('Domain Advanced');

const [dispatch, finance, integrations] = await parallel([

  () => agent(`
You are a senior .NET engineer. Implement the DISPATCH MODULE for the Egypt 3D
Print Platform — the operational core that routes jobs to printer owners and
manages QC.

Solution at E:\\workspace\\3D Printing SAAS\\src\\

CONTEXT: After an order is confirmed, an operator sees a ranked list of certified
printer owners (built in Phase 3 Marketplace module). The operator assigns the job.
The owner accepts or declines within 3 hours. QC is done by the operator via photos
before handing off to Bosta.

DOMAIN ENTITIES (PrintPlatform.Domain/Dispatch/):
JobAssignment.cs (aggregate root)
  - Id (Guid), OrderItemId (FK), PrinterId (FK), PrinterOwnerUserId (FK)
  - Status: enum {
      Offered,          ← operator assigned, waiting for owner response
      Accepted,         ← owner accepted, will print
      Rejected,         ← owner declined → trigger re-routing
      Printing,         ← owner started print
      QCPending,        ← owner uploaded completion photos
      QCApproved,       ← operator approved → move to shipment
      QCRejected,       ← operator rejected → reprint or re-route
      Ready,            ← packaged, ready for Bosta pickup
      Shipped,          ← Bosta collected
      Delivered,        ← Bosta confirmed delivery
      Failed            ← unrecoverable
    }
  - OfferedAt, AcceptedAt, PrintingStartedAt, CompletedAt
  - AcceptanceDeadline (OfferedAt + 3 hours)
  - PayoutAmount (decimal) — AOV minus platform commission
  - PayoutStatus: enum { Pending, Batched, Paid }
  - SlicerFileStorageKey (string — operator uploads slicer file to owner)
  - OperatorNotes
  - RowVersion (byte[] — optimistic concurrency: two operators cannot dispatch same job)
  - Domain events: JobOfferedEvent, JobAcceptedEvent, JobRejectedEvent,
                   QCApprovedEvent, QCRejectedEvent, JobDeliveredEvent

QCRecord.cs
  - Id (Guid), JobAssignmentId (FK)
  - PhotoStorageKeys: List<string> (JSON)
  - OperatorDecision: enum { Approved, Rejected }
  - Notes, ChecklistJson (completed QC checklist from playbook)
  - CreatedAt, OperatorUserId

APPLICATION LAYER (PrintPlatform.Application/Dispatch/):
Commands:
  AssignJobCommand / Handler
    → creates JobAssignment (status=Offered)
    → sends WhatsApp notification to printer owner
    → schedules AcceptanceDeadlineCheckJob (Hangfire, fires in 3h)
    → raises JobOfferedEvent → Gamification.AwardPoints (owner, "JobOffered", 2)
  AcceptJobCommand / Handler — owner accepts → status=Accepted
  RejectJobCommand / Handler — owner rejects → status=Rejected → auto re-route to next candidate
  StartPrintingCommand / Handler — owner marks started → status=Printing
  UploadCompletionPhotosCommand / Handler — owner uploads → status=QCPending
  ApproveQCCommand / Handler
    → creates QCRecord (Approved)
    → status=QCApproved
    → calls Bosta API to create shipment
    → raises QCApprovedEvent → Gamification.AwardPoints (owner, "QCApproved", 20)
  RejectQCCommand / Handler
    → creates QCRecord (Rejected)
    → status=QCRejected
    → decision: reprint (new JobAssignment on same owner) or re-route (new owner)
    → notifies customer of delay
  ConfirmDeliveryCommand / Handler — Bosta webhook → status=Delivered
    → raises JobDeliveredEvent → Gamification + Loyalty events fire

BACKGROUND JOBS (Hangfire):
AcceptanceDeadlineCheckJob
  → if JobAssignment.Status still Offered after AcceptanceDeadline
  → auto-reject, log timeout, re-offer to next ranked printer

BOSTA INTEGRATION STUB (PrintPlatform.Infrastructure/Shipping/):
  IShippingService interface
  BostaShippingService implementing it:
    CreateShipmentAsync(order details) → returns trackingNumber, bostaOrderId
    GetShipmentStatusAsync(bostaOrderId) → returns BostaStatus
    CancelShipmentAsync(bostaOrderId)
  POST /api/shipping/webhook — Bosta delivery status webhook (verify HMAC)

API CONTROLLERS:
DispatchController:
  GET  /api/dispatch/pending           ← operator: orders needing assignment
  POST /api/dispatch/assign            ← operator: assigns job
  POST /api/dispatch/{id}/accept       ← printer owner
  POST /api/dispatch/{id}/reject       ← printer owner
  POST /api/dispatch/{id}/start        ← printer owner
  POST /api/dispatch/{id}/complete     ← printer owner: upload photos (multipart)
  POST /api/dispatch/{id}/qc/approve   ← operator
  POST /api/dispatch/{id}/qc/reject    ← operator
  GET  /api/dispatch/my-jobs           ← printer owner: active + history
  POST /api/shipping/webhook           ← Bosta [AllowAnonymous] + HMAC verify
`, { label: 'domain:dispatch', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior .NET engineer. Implement the FINANCE MODULE for the Egypt 3D
Print Platform — immutable ledger, payout engine, and financial reporting.

Solution at E:\\workspace\\3D Printing SAAS\\src\\

CONTEXT: All money movement is recorded in an append-only ledger — never update,
never delete. Printer owners are paid weekly via InstaPay or bank transfer.
The platform retains its commission. This ledger is the source of truth for
disputes, reconciliation, and owner dashboards.

DOMAIN ENTITIES (PrintPlatform.Domain/Finance/):
LedgerEntry.cs  ← APPEND-ONLY, no updates ever
  - Id (Guid), EntryType: enum {
      CustomerPayment,      ← money received from customer
      PlatformCommission,   ← platform's share retained
      OwnerPayoutBatched,   ← batched into a Payout
      OwnerPayoutPaid,      ← payout actually sent
      Refund,               ← money returned to customer
      AdjustmentCredit,     ← manual correction (Admin only)
      AdjustmentDebit       ← manual correction (Admin only)
    }
  - AmountEgp (decimal), Direction: enum { In, Out }
  - RelatedOrderId (Guid, nullable)
  - RelatedJobAssignmentId (Guid, nullable)
  - RelatedPayoutId (Guid, nullable)
  - PrinterOwnerUserId (Guid, nullable)
  - CustomerUserId (Guid, nullable)
  - Description (string)
  - ExternalReference (string) — Paymob txn id, InstaPay reference, etc.
  - CreatedAt (UTC, set once, never changed)
  - CreatedByUserId (Guid) — operator/system who created entry

Payout.cs
  - Id (Guid), PrinterOwnerUserId (FK)
  - PeriodStart, PeriodEnd (date range)
  - GrossAmountEgp, PlatformFeeEgp, NetAmountEgp
  - Status: enum { Calculating, ReadyToSend, Sent, Failed, Reconciled }
  - PaymentMethod: enum { InstaPay, BankTransfer }
  - ExternalReference (InstaPay txn or bank ref)
  - JobAssignmentIds: List<Guid> (JSON — which jobs included)
  - CreatedAt, SentAt, ReconciledAt

APPLICATION LAYER (PrintPlatform.Application/Finance/):
Commands:
  RecordCustomerPaymentCommand / Handler
    → creates LedgerEntry (CustomerPayment, In)
    → creates LedgerEntry (PlatformCommission, In) — derived amount
  InitiateWeeklyPayoutsCommand / Handler — Hangfire job, runs every Sunday midnight
    → groups unpaid JobAssignments by PrinterOwnerUserId
    → calculates net (AOV - commission)
    → creates Payout (status=ReadyToSend)
    → creates LedgerEntry (OwnerPayoutBatched) per owner
  MarkPayoutSentCommand / Handler — admin/operator marks as sent after manual InstaPay transfer
    → updates Payout.Status=Sent, records ExternalReference
    → creates LedgerEntry (OwnerPayoutPaid)
  RecordRefundCommand / Handler — creates LedgerEntry (Refund, Out)

Queries:
  GetOwnerEarningsQuery / Handler — printer owner dashboard: earnings by period
  GetPlatformRevenueQuery / Handler — admin: GMV, commission, payouts by period
  GetPendingPayoutsQuery / Handler — admin/operator: payouts ready to send
  GetLedgerAuditQuery / Handler — admin: full immutable audit trail with filters

API CONTROLLERS:
FinanceController:
  GET /api/finance/my-earnings                ← PrinterOwner
  GET /api/finance/my-earnings/breakdown      ← PrinterOwner: per-job detail
  GET /api/finance/payouts                    ← Admin/Operator
  POST /api/finance/payouts/{id}/mark-sent    ← Admin/Operator
  GET /api/finance/platform/revenue           ← Admin
  GET /api/finance/ledger                     ← Admin: full audit trail

BACKGROUND JOB:
WeeklyPayoutBatchJob (Hangfire, cron: "0 0 * * 0" — every Sunday midnight)
  → calls InitiateWeeklyPayoutsCommand for all eligible owners
  → sends WhatsApp notification to each owner with their payout amount
  → sends summary to Admin WhatsApp

CRITICAL: All LedgerEntry writes must be in a database transaction with the
triggering entity update (e.g., recording payment + confirming order must be atomic).
Use IDbContextTransaction and ensure no partial states.
`, { label: 'domain:finance', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior .NET engineer. Implement ALL EXTERNAL INTEGRATIONS for the
Egypt 3D Print Platform.

Solution at E:\\workspace\\3D Printing SAAS\\src\\PrintPlatform.Infrastructure\\

Implement these as concrete adapters behind interfaces defined in PrintPlatform.Domain.

─── 1. WHATSAPP BUSINESS API ───
Interface: INotificationService (in Domain)
Concrete: WhatsAppNotificationService (in Infrastructure/Notifications/)

Use Meta Cloud API (graph.facebook.com/v18.0/{phone_number_id}/messages)
Auth: Bearer token from config

Implement message sending methods:
  SendQuoteReadyAsync(customerPhone, quoteDetails) → template: quote_ready
  SendOrderConfirmedAsync(customerPhone, orderDetails) → template: order_confirmed
  SendJobOfferedAsync(ownerPhone, jobDetails) → template: job_offered
  SendQCRejectedAsync(ownerPhone, reason) → template: qc_rejected
  SendQCPhotosForApprovalAsync(operatorPhone, jobId, photoUrls)
  SendShippedAsync(customerPhone, trackingNumber, bostaUrl)
  SendPayoutProcessedAsync(ownerPhone, amount, period)
  SendOtpAsync(phone, otp) → template: otp_verification

All methods: fire-and-forget with retry (Polly: 3 retries, exponential backoff)
Log all sends to Serilog with template name + recipient (no PII in logs)

─── 2. FILE STORAGE (MinIO / S3-compatible) ───
Interface: IFileStorageService (in Domain)
Concrete: MinioFileStorageService (in Infrastructure/Storage/)

Methods:
  UploadAsync(stream, key, contentType, metadata) → returns storageKey
  GetPresignedDownloadUrlAsync(key, expiryMinutes=60) → returns signed URL
  GetPresignedUploadUrlAsync(key, expiryMinutes=30) → for direct browser upload
  DeleteAsync(key)
  ExistsAsync(key) → bool

Key naming convention:
  models/{userId}/{orderId}/{filename}.stl
  qc-photos/{jobAssignmentId}/{timestamp}_{index}.jpg
  slicer-files/{jobAssignmentId}/print_file.gcode

Use AWSSDK.S3 or Minio SDK. Configure via StorageOptions (IOptions<StorageOptions>).

─── 3. BACKGROUND JOB REGISTRATION ───
In Infrastructure/BackgroundJobs/HangfireJobRegistrar.cs:
Register all recurring jobs on startup:
  RecurringJob.AddOrUpdate<WeeklyPayoutBatchJob>("weekly-payouts", j => j.Execute(), "0 0 * * 0")
  RecurringJob.AddOrUpdate<AcceptanceDeadlineCheckJob>("acceptance-check", j => j.Execute(), "*/5 * * * *")
  RecurringJob.AddOrUpdate<LoyaltyPointsExpiryJob>("loyalty-expiry", j => j.Execute(), "0 2 * * *")

─── 4. WHATSAPP WEBHOOK RECEIVER ───
POST /api/webhooks/whatsapp  ← [AllowAnonymous]
  → verify Meta webhook token on GET (hub.challenge response)
  → on POST: verify X-Hub-Signature-256 header
  → parse incoming messages (customer replies to quotes/orders)
  → route to appropriate handler via IMediator

─── 5. HEALTH CHECKS ───
Register ASP.NET health checks for:
  /health/live  — basic liveness
  /health/ready — PostgreSQL + MinIO + WhatsApp reachability
Use Microsoft.Extensions.Diagnostics.HealthChecks

─── 6. SERILOG CONFIGURATION ───
Full Serilog setup in Program.cs:
  - WriteTo.Console (structured JSON in production, pretty in dev)
  - WriteTo.File (rolling daily, 30-day retention)
  - Enrich.WithCorrelationId (Serilog.Enrichers.CorrelationId)
  - Enrich.WithProperty("Application", "PrintPlatform")
  - Filter out health check endpoint noise
  - Log all HTTP requests with StatusCode, ElapsedMs, UserId

─── 7. GLOBAL EXCEPTION HANDLER ───
Implement IExceptionHandler (ASP.NET Core 10 pattern):
  → DomainException → 400 + structured ProblemDetails
  → NotFoundException → 404
  → ConflictException (optimistic concurrency) → 409
  → Unauthorized → 401
  → Unhandled → 500 + log with correlation ID, never leak stack trace
  Always return application/problem+json
`, { label: 'infra:integrations', phase: 'Domain Advanced', schema: RESULT_SCHEMA }),

]);

log(`Domain Advanced done`);

// ─── PHASE 5 — FRONTEND (parallel) ──────────────────────────────────────────
phase('Frontend');

const [frontendCustomer, frontendOperator, frontendOwner] = await parallel([

  () => agent(`
You are a senior React/TypeScript engineer. Build the CUSTOMER-FACING FRONTEND
for the Egypt 3D Print Platform.

Project root: E:\\workspace\\3D Printing SAAS\\frontend\\customer\\
Stack: React 18, TypeScript, Vite, TanStack Query v5, Tailwind CSS,
       React Hook Form, Zod validation, i18next (Arabic RTL + English LTR)

SETUP:
- Vite + React 18 + TypeScript strict mode
- Tailwind with RTL support (dir="rtl" on html when Arabic)
- i18next with ar.json + en.json translation files
- TanStack Query for all server state
- axios instance with JWT interceptor + refresh token logic
- React Router v6

PAGES TO BUILD:

1. /register — Customer registration
   Fields: name (ar), name (en), phone, email, password
   Phone verification OTP step (WhatsApp OTP)

2. /login — Email/phone + password, remember me

3. / (Home) — Landing page matching the concierge playbook copy
   Arabic primary, English toggle, mobile-first
   CTA: "Get a Quote" → /quotes/new
   Secondary CTA: "Own a printer? Join our network" → /printer-owners/register

4. /quotes/new — Multi-step quote request
   Step 1: Upload STL/3MF/OBJ (drag-drop + file picker, max 50MB)
            Show 3D preview using three.js STL loader
   Step 2: Choose material + color + quality preset + quantity
   Step 3: Optional: add design/CAD service
   Step 4: Review instant estimate → "We'll confirm within 2 hours"

5. /quotes — List of my quote requests + status chips

6. /quotes/:id — Quote detail: show confirmed quote, Accept / Decline buttons
   If accepted → redirect to /checkout/:orderId

7. /checkout/:orderId — Payment
   Show order summary
   Payment method selector: COD / InstaPay / Vodafone Cash / Card
   Card: embed Paymob iframe
   COD/InstaPay: show instructions, mark as confirmed

8. /orders — My orders list with status timeline
   Status: Confirmed → In Production → QC Passed → Shipped → Delivered
   Show Bosta tracking link when available

9. /orders/:id — Order detail
   Status timeline component (vertical stepper)
   QC photos (shown after QCApproved)
   Rate & review after delivery

10. /loyalty — Loyalty dashboard
    Current tier badge (Bronze/Silver/Gold/Platinum)
    Points balance, progress to next tier
    Available rewards to redeem
    Points history

COMPONENTS TO BUILD:
- <FileUploader /> — drag/drop with progress bar and STL preview
- <MaterialSelector /> — grid of material cards with properties
- <QualityPresetPicker /> — Draft/Standard/Fine/Ultra with descriptions
- <OrderStatusTimeline /> — vertical stepper with icons
- <LoyaltyTierBadge /> — animated tier display
- <PaymentMethodPicker /> — card UI for payment rail selection
- <LanguageToggle /> — AR/EN switcher in header

All text must have AR + EN translations.
Mobile-first: design for 390px viewport, use Tailwind sm: breakpoints for desktop.
`, { label: 'frontend:customer', phase: 'Frontend', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior React/TypeScript engineer. Build the OPERATOR DASHBOARD for the
Egypt 3D Print Platform.

Project root: E:\\workspace\\3D Printing SAAS\\frontend\\operator\\
Stack: React 18, TypeScript, Vite, TanStack Query v5, Tailwind CSS, Recharts

CONTEXT: Operators are internal staff who review quotes, dispatch jobs to printer
owners, do QC approvals, and manage payouts. This is a desktop-first admin tool.

PAGES TO BUILD:

1. /login — Operator login (role=Operator or Admin)

2. /dashboard — Overview KPIs
   Cards: Pending Quotes, Active Jobs, Awaiting QC, Pending Payouts, Today's Revenue
   Chart: Orders by status (donut), Revenue trend (line, 30 days)

3. /quotes — Quote review queue
   Table: model file preview thumbnail, customer, material, quantity, requested date
   Action: "Open & Confirm Quote" → /quotes/:id/review

4. /quotes/:id/review — Quote confirmation
   Left: 3D model preview (three.js STL loader)
   Right: form to confirm weight, time, price, delivery date, add notes
   Show auto-estimate from geometry analysis as starting point
   Buttons: Confirm Quote | Reject

5. /dispatch — Dispatch queue (confirmed orders awaiting assignment)
   Card per order: customer name, model thumbnail, material, deadline
   Click → /dispatch/:orderId/assign

6. /dispatch/:orderId/assign — Assign job
   Left: order details + model preview
   Right: ranked printer list (from GET /api/printers/available-for-job/:jobId)
     Each row: owner name, printer model, certification level star rating,
               distance to customer, current active jobs, match score bar
   Click printer → Confirm Assignment → POST /api/dispatch/assign

7. /jobs — All active jobs table
   Columns: Job #, Printer Owner, Order, Status chip, Started, ETA
   Filter by status, owner, date range
   Status chip colors: Offered=blue, Printing=orange, QCPending=purple, etc.

8. /jobs/:id — Job detail + QC panel
   Timeline: Offered → Accepted → Printing → QC Pending → Approved → Shipped
   When status=QCPending:
     Show uploaded photos in lightbox grid
     QC checklist (render from playbook checklist items)
     Buttons: Approve QC | Reject QC (with reason modal)

9. /payouts — Payout management
   Table: printer owner, period, gross, fee, net, status
   Filter by status (ReadyToSend / Sent)
   Button: "Mark as Sent" → opens modal to input InstaPay reference number

10. /analytics — Platform analytics
    Revenue by period, top printer owners by volume + rating,
    QC pass rate trend, avg order-to-delivery time

COMPONENTS:
- <ModelPreview3D /> — three.js STL viewer with rotate/zoom
- <RankedPrinterList /> — sortable, with match score visualization
- <QCPhotoGrid /> — lightbox-enabled photo grid
- <StatusChip /> — color-coded status badge
- <JobTimeline /> — horizontal stepper for dispatch flow
- <KpiCard /> — metric card with trend indicator
`, { label: 'frontend:operator', phase: 'Frontend', schema: RESULT_SCHEMA }),

  () => agent(`
You are a senior React/TypeScript engineer. Build the PRINTER OWNER APP for the
Egypt 3D Print Platform — the "driver app" side of the Uber-for-3D-printing.

Project root: E:\\workspace\\3D Printing SAAS\\frontend\\printer-owner\\
Stack: React 18, TypeScript, Vite, TanStack Query v5, Tailwind CSS
Mobile-first (this is used on-the-go by printer owners)

CONTEXT: Printer owners are individuals and small shops in Cairo/Giza who have
idle printer capacity and want to earn money. This app is their interface for:
- Getting notified of new jobs
- Accepting/declining jobs
- Uploading QC photos
- Tracking earnings and gamification progress

PAGES TO BUILD:

1. /register — Printer owner registration
   Fields: name (ar/en), phone, email, password
   Onboarding flow (multi-step after register):
     Step 1: Add first printer (brand, model, technology, build volume, materials)
     Step 2: Location (district dropdown — Cairo/Giza districts list)
     Step 3: Availability (idle hours per day, can pause anytime)
     Step 4: Agreement acceptance (render partner agreement from playbook)
     Step 5: "Pending verification" confirmation screen

2. /login

3. /home — Active job dashboard
   If no active job: "Waiting for jobs... You're available 🟢"
   If job offered: PROMINENT OFFER CARD (3h countdown timer, job details, Accept/Decline)
   If job active: current job status card with next action button

4. /jobs/offered — Job offer detail
   Model info: material, weight estimate, quantity, slicer file download
   Payout amount prominently displayed in green
   Deadline to respond (countdown)
   Buttons: Accept Job | Decline (with reason selector)

5. /jobs/active — Active job tracker
   Status stepper: Accepted → Printing → Photos Uploaded → QC Review → Done
   Current step actions:
     - "Mark as Started Printing"
     - "Upload Completion Photos" → camera/file picker, min 3 photos required
   Slicer file download button
   Customer delivery address (district only — no full address until QCApproved)

6. /jobs — Job history
   Past jobs: date, material, payout, QC result (pass/fail chip)
   Filter: last 7 days / 30 days / all time

7. /earnings — Earnings dashboard
   This week / this month / all time totals
   Next payout date + estimated amount
   Payout history: date, amount, InstaPay reference
   "Your InstaPay number on file: [last 4 digits]" with edit button

8. /profile — My printers + settings
   Printer list: each with status (Active/Paused), toggle to pause
   Add printer button
   Availability toggle (appear in dispatch / go offline)
   Notification preferences

9. /achievements — Gamification page (the fun part)
   Current level badge (Novice → Certified → Expert → Master → Elite) + XP bar
   Next milestone: "X XP to Certified — priority in job queue 🎯"
   Unlocked badges grid with tooltips
   Active challenges + progress bars
   Weekly leaderboard (top 10 owners by jobs completed this week)
   Active streaks: "5 QC approvals in a row 🔥"

COMPONENTS:
- <JobOfferCard /> — high-urgency, countdown timer, accept/decline
- <CountdownTimer /> — real-time countdown to acceptance deadline
- <PhotoUploader /> — camera-first on mobile, min 3 required, shows preview
- <LevelProgress /> — XP bar with level name and next milestone
- <BadgeGrid /> — unlocked vs locked badges (locked are greyscale)
- <LeaderboardRow /> — rank, name, count, highlight if current user
- <StreakCounter /> — fire emoji + count + type
- <EarningsCard /> — period selector, amount, payout date
`, { label: 'frontend:owner', phase: 'Frontend', schema: RESULT_SCHEMA }),

]);

log(`Frontend done`);

// ─── PHASE 6 — WIRE & VERIFY ────────────────────────────────────────────────
phase('Wire & Verify');

const [wiring, devops] = await parallel([

  () => agent(`
You are a senior .NET engineer. Wire all the pieces together in the Egypt 3D Print
Platform and write integration tests.

Solution at E:\\workspace\\3D Printing SAAS\\src\\

TASKS:

1. DOMAIN EVENT WIRING
   In PrintPlatform.Application/EventHandlers/ create handlers that bridge
   domain events → Gamification and Loyalty package calls:

   GamificationEventHandler.cs — handles:
     JobAcceptedEvent → gamification.AwardPointsAsync(ownerUserId, "JobAccepted", 5)
     QCApprovedEvent  → gamification.AwardPointsAsync(ownerUserId, "QCApproved", 20)
     QCRejectedEvent  → gamification.ResetStreakAsync(ownerUserId, "QCApproval")
     JobDeliveredEvent → gamification.AwardPointsAsync(ownerUserId, "OrderDelivered", 10)
     ReviewReceivedEvent → gamification.AwardPointsAsync(ownerUserId, "ReviewReceived", 5)

   LoyaltyEventHandler.cs — handles:
     OrderConfirmedEvent → loyalty.EarnAsync(customerUserId, order.TotalEgp, orderId)
     ReviewLeftEvent → loyalty.EarnAsync(customerUserId, 0, orderId, "ReviewBonus") — fixed 50pts

   GamificationNotificationHandler.cs — handles:
     AchievementUnlockedEvent (from Gamification package)
       → whatsapp.SendAchievementUnlockedAsync(ownerPhone, achievementName)
     LevelUpEvent (from Gamification package)
       → whatsapp.SendLevelUpAsync(ownerPhone, newLevelName, newBenefits)

   LoyaltyNotificationHandler.cs — handles:
     TierUpgradedEvent (from Loyalty package)
       → whatsapp.SendTierUpgradeAsync(customerPhone, newTierName)

2. DBCONTEXT CONFIGURATION
   In PrintPlatform.Infrastructure/Data/AppDbContext.cs:
   - Register all entity configurations from Domain
   - Global query filter: .HasQueryFilter(e => !e.IsDeleted) on all ISoftDelete entities
   - RowVersion concurrency token on Order and JobAssignment
   - Value converters for enums → string storage
   - JSON column support for List<string>, List<Guid> fields

   IMPORTANT: Gamification and Loyalty have their OWN DbContext instances.
   They share the same PostgreSQL server but separate schema prefixes (gam_, loy_).
   Configure via:
     services.AddDbContext<GamificationDbContext>(opts => opts.UseNpgsql(connStr))
     services.AddDbContext<LoyaltyDbContext>(opts => opts.UseNpgsql(connStr))

3. EF CORE MIGRATIONS
   Create initial migration for AppDbContext:
     dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
   Create migration for GamificationDbContext:
     dotnet ef migrations add GamificationInit --context GamificationDbContext ...
   Create migration for LoyaltyDbContext:
     dotnet ef migrations add LoyaltyInit --context LoyaltyDbContext ...
   Create DatabaseSeeder.cs that seeds:
     - MaterialOption rows (all types + common colors)
     - Level rows for Gamification
     - LoyaltyTier rows
     - Admin user (from config, not hardcoded)

4. INTEGRATION TESTS (PrintPlatform.Application.Tests/Integration/)
   Use WebApplicationFactory<Program> + real PostgreSQL (Testcontainers)
   Write tests for the happy-path flows:

   OrderFlowIntegrationTest:
     - Register customer → upload model → request quote → operator confirms →
       customer accepts → payment webhook → job assigned → owner accepts →
       photos uploaded → QC approved → Bosta webhook → delivered
     Assert: LedgerEntry count = 2 (payment + commission), Order.Status = Delivered

   GamificationFlowIntegrationTest:
     - QCApproved event fires → GamificationPlayer.TotalXP increases by 20
     - 10 consecutive QCApproved → AchievementUnlocked event raised (Perfectionist)

   LoyaltyFlowIntegrationTest:
     - OrderConfirmed (1200 EGP) → 120 loyalty points earned
     - Accumulate to 1000 pts → TierUpgraded to Silver

5. OPENAPI / SWAGGER
   Add XML documentation comments to all controllers
   Configure Swagger UI at /swagger with JWT bearer input
   Export openapi.json to E:\\workspace\\3D Printing SAAS\\docs\\openapi.json
`, { label: 'wire:events-tests', phase: 'Wire & Verify', schema: RESULT_SCHEMA }),

  () => agent(`
You are a DevOps engineer. Set up the complete deployment infrastructure for the
Egypt 3D Print Platform.

Solution at E:\\workspace\\3D Printing SAAS\\

TASKS:

1. DOCKERFILE (already scaffolded — finalize it)
   Verify multi-stage build works:
     Stage 1 (build): mcr.microsoft.com/dotnet/sdk:10.0, restore + build + test
     Stage 2 (publish): dotnet publish -c Release -o /app/publish
     Stage 3 (runtime): mcr.microsoft.com/dotnet/aspnet:10.0, non-root user
   EXPOSE 8080
   HEALTHCHECK --interval=30s CMD curl -f http://localhost:8080/health/live

2. DOCKER COMPOSE (finalize)
   Services:
     api:
       build: .
       ports: "8080:8080"
       environment: from .env file
       depends_on: postgres, minio
       restart: unless-stopped
     postgres:
       image: postgres:16-alpine
       volumes: pgdata:/var/lib/postgresql/data
       environment: POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD
     minio:
       image: minio/minio
       command: server /data --console-address ":9001"
       ports: "9000:9000", "9001:9001"
       volumes: miniodata:/data
     hangfire-dashboard: (separate service or embedded in api)
   volumes: pgdata, miniodata
   networks: internal bridge

3. .env.example file with ALL required env vars (no real values):
   DATABASE_URL=
   MINIO_ENDPOINT=
   MINIO_ACCESS_KEY=
   MINIO_SECRET_KEY=
   JWT_SECRET=
   PAYMOB_API_KEY=
   PAYMOB_INTEGRATION_ID=
   BOSTA_API_KEY=
   WHATSAPP_TOKEN=
   WHATSAPP_PHONE_NUMBER_ID=
   ADMIN_EMAIL=
   ADMIN_PASSWORD=

4. GITHUB ACTIONS CI (.github/workflows/ci.yml — finalize):
   Triggers: push to main, PR to main
   Jobs:
     test:
       runs-on: ubuntu-latest
       services:
         postgres: image postgres:16
         minio: image minio/minio (use bitnami/minio for CI friendliness)
       steps: checkout, setup-dotnet 10, restore, build, test (with --logger trx)
     docker:
       needs: test (only on push to main)
       steps: checkout, docker/login-action (GHCR), docker/build-push-action
              tags: ghcr.io/{owner}/print-platform:latest, :{sha}

5. DEPLOYMENT GUIDE (docs/DEPLOY.md):
   - Hetzner CX22 VPS setup (Ubuntu 22.04)
   - Docker + Docker Compose install
   - Clone repo, copy .env, docker compose up -d
   - Nginx reverse proxy config with SSL (Let's Encrypt / Certbot)
   - Apply EF migrations: docker exec api dotnet ef database update
   - Run database seeder
   - Verify: /health/ready endpoint
   - Bosta + Paymob webhook URL registration steps

6. MAKEFILE for developer convenience:
   make dev        — docker compose up (with hot reload via volume mount)
   make test       — run all tests
   make migrate    — apply EF migrations inside container
   make seed       — run database seeder
   make logs       — follow api service logs
   make clean      — stop + remove containers + volumes
`, { label: 'devops:docker-cicd', phase: 'Wire & Verify', schema: RESULT_SCHEMA }),

]);

// ─── FINAL SUMMARY ──────────────────────────────────────────────────────────
const allResults = [scaffold, gamification, loyalty, identity, marketplace,
                    orders, dispatch, finance, integrations,
                    frontendCustomer, frontendOperator, frontendOwner,
                    wiring, devops].filter(Boolean);

const totalFiles = allResults.reduce((sum, r) => sum + (r?.filesCreated?.length ?? 0), 0);
const openItems  = allResults.flatMap(r => r?.openItems ?? []);

log(`Implementation complete — ${totalFiles} files across 14 agents, ${openItems.length} open items`);

return {
  totalFiles,
  agents: 14,
  phases: 6,
  openItems,
  summary: 'Egypt 3D Print Platform built: modular monolith + Gamification package + Loyalty package + 3 frontends + DevOps',
};
