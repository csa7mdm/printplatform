# Loyalty Points Expiry Logic

## Overview
This document explains the standard FIFO (First-In, First-Out) expiration logic implemented for the PrintPlatform Loyalty module. The process adheres to MENA market standards, meaning individual point batches expire based on the time they were earned, rather than clearing a member's entire balance due to inactivity.

## How it Works

### Earning Points (`EarnAsync`)
When a member earns base points or receives a bonus, a new `PointsLedger` entry is created. If the `ExpiryMonths` configuration is set to a value greater than 0, the `Expiry` date is calculated as `CreatedAt.AddMonths(ExpiryMonths)`.

### Redeeming Points (`RedeemAsync`)
The ledger operates on an append-only basis to preserve a complete history of events. When points are redeemed, a single ledger entry with a negative delta (`LedgerEntryType.Redeemed`) is added. It does not map explicitly to which `Earned` batch of points was spent; we rely on the FIFO calculation to reconcile this during the expiration process.

### Processing Expiries (`ProcessExpiriesAsync`)
The Hangfire job `loyalty-expiry` runs daily (`0 2 * * *`) and dispatches the `ProcessLoyaltyExpiryCommand`.

Inside `LoyaltyService.ProcessExpiriesAsync`:
1. It queries members who have an `ActivePoints` balance > 0.
2. For each member, it pulls all ledger entries.
3. It identifies the total number of points that are subject to expiry (`totalEarnedSubjectToExpire`), meaning `Delta > 0` and their `Expiry` date has passed.
4. It computes the total points that have already been deducted from the member's lifetime balance (i.e. all Redemptions and previously Expired points).
5. Using FIFO, any spent points inherently offset the oldest earned points. Thus, the total points that should be expired right now is:
   `totalEarnedSubjectToExpire - totalDeducted`.
6. If this value is greater than 0, the member's `ActivePoints` are deducted up to their available balance, and a new ledger entry (`LedgerEntryType.Expired`) with a negative delta is created to reflect the expiration.

This approach guarantees an immutable append-only ledger while keeping track of unspent, expired points accurately based on FIFO rules.
