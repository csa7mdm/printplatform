# Concierge MVP Playbook — Week 1
Egypt 3D Print Platform · No code required · Goal: first paying order within 7 days

---

## Day-by-Day Schedule

| Day | Focus | Done when |
|-----|-------|-----------|
| 1 | Setup: domain, WhatsApp Business, landing page | Landing page live, WhatsApp number active |
| 2 | Supply: find and call 2 printer owners | At least 1 partner confirmed |
| 3 | Demand: outreach batch 1 — 10 engineering/arch firms | Messages sent, responses tracked |
| 4 | Demand: outreach batch 2 — Facebook groups + LinkedIn | Posts live |
| 5 | Follow-up + handle first inquiries | At least 1 quote conversation open |
| 6 | Close first order | Deposit or COD confirmed |
| 7 | Fulfill + debrief | Part delivered, kill-criteria sheet updated |

---

## 1. Landing Page Copy

> Host on Framer free tier, Carrd, or plain HTML on Netlify. Mobile-first. One CTA only.

---

### Arabic (primary)

**Headline:**
طباعة ثلاثية الأبعاد موثوقة — بجودة مضمونة وتسليم سريع

**Subheadline:**
نربطك بشبكة من أصحاب الطابعات المعتمدين في القاهرة.
عرض سعر خلال ساعتين. تسليم عبر بوسطة.

**Three value bullets:**
- جودة مضمونة أو نعيد الطباعة مجاناً
- طباعة FDM وراتنج (Resin) لكل الاحتياجات
- فاتورة واحدة، جهة واحدة مسؤولة

**CTA button:**
احصل على عرض سعر الآن ← (opens WhatsApp)

**Secondary CTA (printer owners):**
عندك طابعة؟ اكسب دخلاً إضافياً — انضم للشبكة

---

### English (toggle)

**Headline:**
Reliable 3D Printing in Cairo — Quality Guaranteed

**Subheadline:**
A vetted network of certified printer owners.
Quote in 2 hours. Delivered by Bosta.

**Three value bullets:**
- QC verified before it ships — or we reprint free
- FDM and Resin for every application
- One vendor, one invoice, one accountable party

**CTA button:**
Get a Quote on WhatsApp →

**Secondary CTA:**
Own a printer? Earn passive income — join our network

---

## 2. WhatsApp Intake Script

> Save as a WhatsApp Business quick-reply template. Use this word-for-word for the first 20 conversations.

---

### Greeting (auto-reply on first message)

**AR:**
أهلاً! شكراً للتواصل مع [اسم المنصة].
عشان نقدر نجهزلك عرض سعر دقيق، محتاجين منك:

1. ملف STL أو 3MF أو OBJ الخاص بطلبك
2. الخامة المطلوبة (PLA / PETG / راتنج / غيره)
3. الكمية
4. تاريخ التسليم المطلوب
5. محافظتك (للتحقق من التغطية)

هنرد عليك بعرض سعر تفصيلي في خلال ساعتين ✓

**EN:**
Hi! Thanks for reaching out.
To prepare your quote, please send us:

1. Your STL / 3MF / OBJ file
2. Material (PLA / PETG / Resin / other)
3. Quantity
4. Required delivery date
5. Your area in Cairo/Giza

We'll reply with a detailed quote within 2 hours ✓

---

### Quote message (send after reviewing file)

**AR:**
عرض السعر لطلبك:

- الخامة: [PLA/PETG/Resin]
- الوزن التقريبي: [X] جرام
- وقت الطباعة: ~[X] ساعات
- **السعر: [X] جنيه** (شامل التغليف)
- التسليم (بوسطة): [X] جنيه
- **الإجمالي: [X] جنيه**
- موعد التسليم المتوقع: [يوم/تاريخ]

طرق الدفع: كاش عند الاستلام / إنستاباي / فودافون كاش / كارت

العرض ساري لمدة 48 ساعة.
تأكيد الطلب؟ 🖨️

**EN:**
Your quote:

- Material: [PLA/PETG/Resin]
- Estimated weight: [X]g
- Print time: ~[X] hours
- **Print price: [X] EGP** (incl. packaging)
- Bosta delivery: [X] EGP
- **Total: [X] EGP**
- Expected delivery: [day/date]

Payment: COD / InstaPay / Vodafone Cash / Card

Quote valid 48 hours.
Confirm order? 🖨️

---

### Order confirmation + payment

**AR:**
ممتاز! عشان نأكد طلبك:
- اسمك الكامل؟
- عنوان التسليم الكامل؟
- رقم موبايل للبوسطة؟
- طريقة الدفع المفضلة؟

لو كاش عند الاستلام — البوسطة بتحصّل معك.
لو إنستاباي — [رقمك هنا] (اسم الحساب: [X])

**EN:**
Great! To confirm your order:
- Full name?
- Full delivery address?
- Mobile number for Bosta?
- Preferred payment method?

COD: collected by Bosta on delivery.
InstaPay: [your number] (account name: [X])

---

### QC photo message (before shipping)

**AR:**
طلبك اتطبع وعدى مراجعة الجودة ✓
شوف الصور قبل ما نبعت — كل حاجة تمام؟
لو في أي ملاحظة قولنا دلوقتي.
[attach 3–4 QC photos]

**EN:**
Your order has been printed and QC approved ✓
Check the photos before we ship — all good?
Any concerns, let us know now.
[attach 3–4 QC photos]

---

### Printer owner inquiry response

**AR:**
أهلاً! كويس إنك تواصلت.
عندنا شبكة من أصحاب الطابعات المعتمدين — بنبعتلك الطلبات وانت بتطبع وبتتقبض أسبوعياً عن طريق إنستاباي.

عشان نشوف لو طابعتك مناسبة للشبكة:
1. نوع الطابعة وموديلها؟
2. الخامات اللي بتطبع بيها؟
3. حجم البناء (Build Volume)؟
4. محافظتك؟
5. تقريباً كام ساعة يومياً الطابعة فاضية؟

**EN:**
Hi! Glad you reached out.
We run a vetted printer network — we send you jobs, you print, we pay weekly via InstaPay.

To check if your printer fits:
1. Printer brand and model?
2. Materials you support?
3. Build volume?
4. Your area?
5. Roughly how many idle hours per day?

---

## 3. Partner Printer Owner Agreement (1-pager)

> Print, sign, scan. Keep it simple — save lawyers for later.

---

**اتفاقية شراكة — شبكة الطباعة ثلاثية الأبعاد**
**Partner Printer Agreement**

بين [اسم المنصة] ("المنصة") و [اسم صاحب الطابعة] ("الشريك")
Between [Platform Name] ("Platform") and [Printer Owner Name] ("Partner")

**الشريك يوافق على / Partner agrees to:**

1. قبول أو رفض الطلبات خلال 3 ساعات من الإرسال
   Accept or decline jobs within 3 hours of assignment
2. طباعة الطلبات بمواصفات التقسيم المرسلة بدون تعديل
   Print to exact provided slicer specs without modification
3. رفع 3 صور على الأقل عند اكتمال الطباعة قبل التسليم
   Upload minimum 3 photos upon print completion before handoff
4. تسليم الطلبات لسائق بوسطة خلال الموعد المتفق عليه
   Hand off to Bosta courier by agreed time
5. الحفاظ على نسبة قبول ≥ 80% ونسبة اجتياز مراجعة الجودة ≥ 90%
   Maintain acceptance rate ≥ 80% and QC pass rate ≥ 90%

**المنصة تلتزم بـ / Platform commits to:**

1. إرسال طلبات واضحة مع ملف التقسيم والمواصفات الكاملة
   Send clear jobs with slicer file and full specifications
2. الدفع أسبوعياً عن طريق إنستاباي أو تحويل بنكي
   Pay weekly via InstaPay or bank transfer
3. عمولة المنصة [15–25]% من قيمة الطلب — الباقي للشريك
   Platform takes [15–25]% commission — remainder to partner
4. تحمّل تكلفة إعادة الطباعة في حالة أخطاء التقسيم
   Cover reprint cost for any slicer-file errors on our side

**الفسخ / Termination:**
يحق لأي طرف إنهاء الاتفاقية بإشعار 48 ساعة مكتوب عبر واتساب.
Either party may terminate with 48-hour written WhatsApp notice.

التوقيع / Signature: _____________ التاريخ / Date: _____________

---

## 4. QC Checklist (per job, before approving shipment)

> Operator completes this for every job. Attach photos to WhatsApp thread.

```
JOB QC CHECKLIST
Order #: ________  Date: ________  Printer Owner: ________

DIMENSIONAL
[ ] Overall dimensions within ±0.5mm of spec
[ ] Critical features (holes, slots, mating surfaces) within ±0.3mm
[ ] No warping — part sits flat on flat surface

SURFACE QUALITY
[ ] No visible layer delamination
[ ] No stringing or blobs on visible faces
[ ] Supports removed cleanly — no scars on critical surfaces
[ ] First layer adhesion marks acceptable (bottom face)

STRUCTURAL
[ ] No cracks at layer lines
[ ] No under-extrusion voids visible
[ ] Infill density feels correct (squeeze test on flexible parts)

RESIN SPECIFIC (if applicable)
[ ] Fully cured — no soft or tacky spots
[ ] Support scars sanded smooth on visible faces
[ ] No IPA residue (white haze)

PACKAGING
[ ] Part wrapped in bubble wrap
[ ] Fragile label if required
[ ] Order slip inside package

PHOTOS TAKEN (attach to order)
[ ] Top view
[ ] Side view
[ ] Critical feature close-up
[ ] Packaged and ready

DECISION
[ ] APPROVED — proceed to Bosta
[ ] REJECTED — reprint required

Reason for rejection (if any): ________________________
Operator: ________________  Time: ________________
```

---

## 5. B2B Outreach Message

> Send via LinkedIn DM, email, or WhatsApp. Personalise [FIRM] and [SPECIFIC_USE].

**AR (WhatsApp/email):**

Subject: طباعة ثلاثية الأبعاد موثوقة لـ [FIRM]

أهلاً،
بتتواصل معاك [اسمك] من [اسم المنصة].

بنقدم خدمة طباعة ثلاثية الأبعاد متخصصة للشركات الهندسية في القاهرة —
نماذج وظيفية، جيجز، تركيبات، وقطع بروتوتايب بضمان الجودة وموعد تسليم مضمون.

الفرق عننا: مش بس خدمة طباعة — عندنا شبكة طابعات موزعة في القاهرة، يعني طاقة أعلى وتسليم أسرع، مع مراجعة جودة قبل كل شحنة وفاتورة واحدة.

لو عندكم طلبات طباعة حالية أو قادمة، يسعدني أرسلك نموذج مجاني لأي قطعة لتجربة الجودة بنفسك.

[اسمك] | [رقم واتساب]

---

**EN:**

Subject: Reliable 3D Printing for [FIRM]

Hi [Name],

I'm [your name] from [Platform Name].

We provide a professional 3D printing fulfillment service for engineering and product firms in Cairo — functional prototypes, jigs, fixtures, and production parts with QC guarantee and committed lead times.

What's different: we run a distributed network of certified printers across Cairo, which means higher capacity, faster turnaround, and QC sign-off before every shipment — with one invoice.

If you have any current or upcoming print requirements, I'd be happy to send a free sample part so you can check quality first-hand.

[Your name] | [WhatsApp number]

---

## 6. Kill Criteria — Decision Gate

Review after first 15 orders (or 30 days, whichever comes first).

| Metric | Kill threshold | Keep building |
|--------|---------------|---------------|
| B2B repeat rate (reorder ≤30 days) | < 20% | ≥ 30% |
| Blended AOV | < 500 EGP | ≥ 900 EGP |
| Design/CAD attach rate | < 10% | ≥ 20% |
| Premium willingness (paid more than cheapest quote) | 0 of 5 asked | ≥ 3 of 5 |
| Supply-side joins (printer owners who passed screening) | < 2 | ≥ 3 |

**If all five pass → build the platform.**
**If two or more fail → pivot the segment or the offer before building.**

---

## Tools Needed (all free at this stage)

| Tool | Purpose | Cost |
|------|---------|------|
| Carrd.co or Framer | Landing page | Free |
| WhatsApp Business app | Intake + comms | Free |
| Google Sheets (kill criteria tracker) | Order tracking | Free |
| Notion or Google Docs | Partner agreements + QC records | Free |
| Bosta account | Shipping + COD | Per shipment |
| Personal InstaPay | Receive digital payments | Free |
| Domain (.com or .eg) | Professional link in bio | ~500–1500 EGP |
