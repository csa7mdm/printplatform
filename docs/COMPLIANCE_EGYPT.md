# PrintPlatform — Egypt Regulatory & Compliance Notes

Non-exhaustive checklist. Validate each item with Egyptian counsel/accountant.

## 1. Business registration (required before taking payments)
- **Commercial register + tax card** in the owner's name or a company (e.g. one-
  person company "شركة الشخص الواحد").
- **VAT registration** if annual turnover exceeds the threshold (EGP 500,000 at
  time of writing) — verify the current threshold with the Egyptian Tax Authority
  (ETA).

## 2. E-invoicing (ETA)
Egypt mandates e-invoicing (الفاتورة الإلكترونية) for VAT-registered businesses,
rolled out in phases. Once you are VAT-registered you must integrate with ETA's
e-invoicing system (SDK/API) and issue electronic invoices with a UUID/QR.
- Plan for it before you scale B2B volumes, since engineering firms expect
  tax-compliant invoices.

## 3. Payments (CBE / PSP)
- Use a licensed payment facilitator for cards (e.g. **Paymob**) — never store
  full card numbers (PCI DSS). The platform currently targets Paymob for cards.
- **InstaPay** (CBE instant payments) and mobile wallets (Vodafone Cash) are
  licensed rails; payout flows must reconcile with CBE rules.
- Keep auditable records of all settlements and payouts for ETA/CBE inspection.

## 4. Data protection (Law 151/2020)
- Egypt's Personal Data Protection Law (Law 151/2020) and its executive
  regulations govern personal data. Register with the Personal Data Protection
  Centre where applicable; honor access/correction/deletion requests.
- National IDs are sensitive data — the platform encrypts them at rest; ensure
  access is limited and logged.

## 5. Consumer protection (Law 181/2018)
- Clear pricing in EGP, delivery terms, and refund rights.
- COD is regulated; ensure courier (Bosta) COD reconciliation.

## 6. Content / safety
- Screen uploaded models for prohibited items (e.g. firearms) per Egyptian law.

## 7. Recommended additions to the repo
- Publish `PRIVACY_POLICY.md` and `TERMS_OF_SERVICE.md` on the public site.
- Add a `SECURITY.md` (responsible disclosure) once public-facing.
- Add `LICENSE` (BUSL-1.1 provided) so reuse terms are explicit.