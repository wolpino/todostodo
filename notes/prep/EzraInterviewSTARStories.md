# Ezra Interview — STAR Stories

**Format:** Situation → Task → Action → Result (~90 seconds spoken each)  
**Lead with impact.** End with Ezra tie-in when natural.

Who was your customer? What was the goal? Why did this project exist?

---

## Story 1 — Production incident: Zapier user deletion failures

**Use for:** "Tell me about a production issue," "How do you handle incidents," "Data integrity"


|                 |                                                                                                                                                                                                                                                                                                                                                                                                                       |
| --------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Situation**   | At Zapier, we had a backlog of **2,000+ failed user deletions**. This wasn't just support noise — it blocked us from passing **SOC2 audits** under **GDPR and CCPA** requirements for identity management across the entire product (3M+ users).                                                                                                                                                                      |
| **Task**        | I owned diagnosing why deletions were failing and making the process reliable enough for compliance sign-off.                                                                                                                                                                                                                                                                                                         |
| **Action**      | I categorized failure modes — downstream integration timeouts, partial state, retry logic gaps. I traced the deletion pipeline across our identity stack, identified brittle points, and implemented fixes so deletions either completed fully or failed in a recoverable, observable way. I worked with product and compliance stakeholders so the fix matched audit expectations, not just engineering convenience. |
| **Result**      | We cleared the failure pattern and **passed the audits**. User deletion became something the company could trust at scale.                                                                                                                                                                                                                                                                                            |
| **Ezra tie-in** | Member data lifecycle — retention, deletion, audit trails — is the same class of problem at higher stakes. I'd treat a PHI deletion bug as a SEV, not a backlog item.                                                                                                                                                                                                                                                 |


### 60-second version (if interrupted)

> Zapier had 2000+ failed user deletions blocking SOC2/GDPR/CCPA audits. I traced the identity deletion pipeline, categorized failure modes, fixed brittle retry and partial-state paths, and aligned with compliance on what "deleted" meant. Audits passed. Same severity framing I'd use for member imaging data at Ezra.

---

## Story 2 — Integration reliability: Lighter Capital webhooks

**Use for:** "Improve reliability," "API integration," "Technical decision you drove," C#/.NET stack proof


|                 |                                                                                                                                                                                                                        |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Situation**   | At Lighter Capital, our third-party financial data integration had a high error rate from **polling-based sync** — stale data, missed updates, flaky retries. Ops did manual quarterly reconciliation.                 |
| **Task**        | Redesign the integration to be event-driven and observable.                                                                                                                                                            |
| **Action**      | I led development of a modular pipeline using **AWS SNS, Lambdas, and S3** to ingest webhook events, transform data, and fit it into our existing SQL schema. Built in **C# and React** — same backend family as Ezra. |
| **Result**      | **60% reduction in integration errors.** Ops stopped manual quarterly reconciliation.                                                                                                                                  |
| **Ezra tie-in** | Scheduling and imaging partner integrations need the same mindset — idempotent handlers, clear failure modes, monitoring when an external system is the source of truth.                                               |


### 60-second version

> Lighter Capital's financial data integration failed often due to polling. I redesigned it as an event-driven pipeline — SNS, Lambdas, S3 — in C# and React. 60% error reduction. Same pattern Ezra needs for imaging center and scheduling partner integrations.

---

## Story 3 — Fintech compliance: Zapier identity + SOC2/GDPR/CCPA

**Use for:** "Security or compliance challenge," "Regulated environment," "HIPAA-adjacent"


|                 |                                                                                                                                                                                                                                                                                                                                                                           |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Situation**   | Identity management at Zapier touches every product surface. A deletion or access bug isn't a feature bug — it's a **compliance incident**. We needed to pass SOC2 audits with GDPR/CCPA controls for user data lifecycle.                                                                                                                                                |
| **Task**        | Ensure identity features (registration, deletion, access control) met audit requirements while shipping product improvements.                                                                                                                                                                                                                                             |
| **Action**      | I treated GDPR/CCPA requirements as **design constraints**, not paperwork. Every change considered: who can trigger this, what's logged, can we prove deletion happened? When 2000+ deletions failed, I prioritized it as a compliance blocker, not a backlog item. Collaborated with product and design so security constraints didn't create UX users would circumvent. |
| **Result**      | Audits passed. Identity management became reliable at scale. Established pattern: compliance-critical paths get tests, monitoring, and explicit ownership.                                                                                                                                                                                                                |
| **Ezra tie-in** | HIPAA is the same muscle — least privilege, audit trails, provable data handling. I haven't worked with PHI in software, but I have at Klick-adjacent thinking through Zapier SOC2 and clinical front-desk experience with patient data handling.                                                                                                                         |


### Take-home supplement (use with Story 3 or alone)


|               |                                                                                                                |
| ------------- | -------------------------------------------------------------------------------------------------------------- |
| **Situation** | Early in the take-home, entries weren't scoped by UserId — any authenticated user could see all todos.         |
| **Action**    | Class-level `[Authorize]`, UserId from claims only, 404 for cross-user access, dedicated `AuthorizationTests`. |
| **Result**    | Regression tests prevent recurrence. Same fail-closed pattern I'd use for member imaging data.                 |


---

## Bonus stories (if needed)

### Carta company merge — end-to-end ownership


|               |                                                                                 |
| ------------- | ------------------------------------------------------------------------------- |
| **Situation** | 3-hour manual company merge process, error-prone                                |
| **Action**    | Designed Postgres changes, Django/React flow, full test coverage for edge cases |
| **Result**    | One-click merge, **~1,080 associate hours saved/year**                          |
| **Use for**   | "Feature you owned end-to-end," "How do you work with stakeholders"             |


### C# ramp / AI reset — learning and judgment


|               |                                                                                      |
| ------------- | ------------------------------------------------------------------------------------ |
| **Situation** | Hadn't written C# in 5 years; AI generated code faster than I could review           |
| **Action**    | Wiped repo, scaffolded manually, small commits, tests first, documented in DESIGN.md |
| **Result**    | Second pass was faster and fully explainable                                         |
| **Use for**   | "Unfamiliar stack," "Mistake you made," "How do you use AI"                          |


---

## Question → Story quick map


| Question                | Story                                                           |
| ----------------------- | --------------------------------------------------------------- |
| Production incident     | Story 1 (Zapier deletions)                                      |
| Integration reliability | Story 2 (Lighter webhooks)                                      |
| Compliance / security   | Story 3 (Zapier SOC2) + take-home UserId                        |
| End-to-end ownership    | Carta merge                                                     |
| Unfamiliar stack / AI   | C# ramp                                                         |
| Disagree with decision  | Lighter: polling → webhooks proposal                            |
| Why Ezra                | Mission + clinical + fintech (no single STAR — weave all three) |


---

## Incident worksheet (filled in)

### Incident 1 — Zapier user deletion failures

- **S:** 2000+ failed deletions; SOC2/GDPR/CCPA audit blocker; identity management across 3M+ users
- **T:** Owner of root-cause fix and compliance alignment
- **A:** Categorized failure modes → traced pipeline → fixed retry/partial-state → stakeholder alignment
- **R:** Audits passed; reliable deletion at scale
- **Ezra:** PHI deletion = same SEV framing

### Incident 2 — Lighter Capital integration errors

- **S:** High error rate on polling-based financial data sync; manual quarterly reconciliation
- **T:** Redesign for reliability
- **A:** Webhook pipeline (SNS/Lambda/S3); idempotent handlers; C#/SQL integration
- **R:** 60% error reduction
- **Ezra:** Partner integration reliability for imaging/scheduling

