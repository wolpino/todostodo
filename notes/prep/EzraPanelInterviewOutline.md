# Ezra Panel Interview — At-a-Glance Outline

**Panel:** [Kristina Matiukhina](https://www.linkedin.com/in/kristina-matiukhina/) (Team Lead) · [Michael Hambor](https://www.linkedin.com/in/michaelhambor/) (Staff Engineer)  
**Format:** Behavioral + situational + ~30 min take-home walkthrough + "what if" follow-ups  
**Your anchors:** Resume stories (Zapier/Carta/Lighter) · Take-home (DESIGN.md) · Clinical empathy

**Full prep docs:**
| Doc | Contents |
|-----|----------|
| [EzraInterviewCodeWalkthrough.md](EzraInterviewCodeWalkthrough.md) | **"Walk me through your code"** — bulleted spoken outline |
| [EzraInterviewNarrative.md](EzraInterviewNarrative.md) | 2-min script + 10-min walkthrough |
| [EzraInterviewSTARStories.md](EzraInterviewSTARStories.md) | STAR stories (Zapier, Lighter, compliance) |
| [EzraInterviewCodeTour.md](EzraInterviewCodeTour.md) | EntryController, useEntries, tests |
| [EzraInterviewWhatIfs.md](EzraInterviewWhatIfs.md) | 12+ what-if answers |
| [EzraInterviewQuestions.md](EzraInterviewQuestions.md) | Questions for Kristina & Michael |
| [EzraInterviewLiveDemo.md](EzraInterviewLiveDemo.md) | Test results + manual UI checklist |
| [EzraInterviewQA.md](EzraInterviewQA.md) | Ad-hoc prep Q&A (this conversation + future questions) |

**Role description:** [EzraRecruiterDetails.md](EzraRecruiterDetails.md) — imaging team Senior Full Stack (recruiter copy)

---

## Role Expectations — Imaging Team (from recruiter JD)

*Only what isn't already in the stories/walkthrough above.*

| They want | Your proof / honest gap |
|-----------|-------------------------|
| **Technical leader** on cancer-screening web platforms | Take-home: owned architecture, DESIGN.md, CI/Docker; Carta merge led end-to-end |
| **Vue + .NET in production** | Their stack is Vue; your take-home is React + .NET — same patterns (typed API client, query cache). Ramp plan: pair early, read pod components first |
| **APIs integrating EMRs, imaging partners, AI services** | Lighter webhooks + Carta third-party API = external-system integration muscle. EMR/HL7/DICOM: no software exp yet — clinical front-desk + eager to learn; ask Kristina how partner integrations are structured |
| **High-performance ASP.NET Core services** | Take-home is CRUD-scale; call out pagination/index path. Lighter was C# + SQL performance work |
| **SQL Server depth** | Take-home: SQLite in-mem. Carta/Lighter: Postgres + SQL. Frame as: relational modeling transfers; SQL Server specifics I'd pick up on the job |
| **Azure DevOps / IaC / K8s** | Resume: Kubernetes, AWS (Lighter SNS/Lambda/S3). Azure is a ramp — cloud patterns transfer (pipelines, containers, monitoring) |
| **Microservices + event-driven + domain boundaries** | Lighter event pipeline; Zapier identity as bounded context across product surfaces |
| **Mentor via code review, pairing, design sessions** | Zapier cross-team with product/design; glue work + docs on resume — cite a specific review that taught someone something if asked |
| **Product + design + clinical collaboration** | Clinical background (phlebotomy, urgent care intake, OBGYN reception) + Zapier identity UX with compliance constraints |
| **Desktop, tablet, mobile experiences** | Take-home: desktop-first narrow shell; mobile settings drawer. Gap: not primary design target — Chakra gives a11y baseline |
| **Test strategy: unit, integration, e2e** | Have: xUnit (65), Vitest (17), integration auth/CORS tests. Gap: no Playwright yet — on your "one more week" list |
| **Performance tuning + observability** | Have: Serilog + structured logging. Gap: no metrics/tracing in take-home — name it |
| **Influence engineering strategy / roadmap** | Carta merge replaced manual ops; at Zapier worked identity across product — propose, document, get buy-in |

### Nice-to-have — quick hooks

| Nice-to-have | You |
|--------------|-----|
| DICOM / HL7 / FHIR | No SWE experience — understand why they matter for imaging/scheduling; willing to ramp |
| HIPAA / SOC 2 | Zapier SOC2/GDPR/CCPA (Story A); Kristina did HIPAA at Klick — good question for her |
| PWA / accessibility | Chakra UI built-in a11y; responsive layout in take-home |
| Python / GCP / FastAPI | Zapier + Carta Python; future-initiative plus, not blocker |

### One-liner if they ask "why imaging team specifically"

> I want to build the member-facing platforms that make screening trustworthy — scheduling, results, the experience around the MRI — not just backend plumbing. My clinical background means I care about how software feels to anxious patients, and my fintech compliance work means I won't cut corners on data handling.

---

## 60-Second Opener

> Production-minded todo MVP — real auth, UserId isolation, tests, Docker, CI, logging. Reset once when AI got ahead of my C# review; rebuilt with small diffs. Cookie sessions, OpenAPI codegen, optimistic React Query. Minimal-click UX from clinical/front-desk experience. Documented tradeoffs and scale path in DESIGN.md.

---

## Take-Home Walkthrough (10 min — hit these in order)

| # | Topic | One line | File / detail |
|---|-------|----------|---------------|
| 1 | Interpretation | CRUD + lifecycle + auth + ops hygiene | DESIGN.md §1 |
| 2 | Architecture | Decoupled SPA + API, OpenAPI contract | Program.cs, main.tsx |
| 3 | Security | UserId from claim only; 404 not 403; class `[Authorize]` | EntryController.cs |
| 4 | Tradeoffs | Cookies, SQLite in-mem, soft delete, REST | DESIGN.md §3 |
| 5 | UX | Composer, forward-only status, pop-out + BroadcastChannel | useEntries.ts |
| 6 | Ops | CI (3 jobs), Docker single port, Serilog, ProblemDetails | ci.yml, GlobalExceptionHandler |
| 7 | Gaps | No pagination, no migrations, no Redis sessions | DESIGN.md §5, §8 |
| 8 | Ezra bridge | Same isolation principle as member imaging data | — |

---

## Security Story (know cold)

```
Every query: WHERE UserId == claim AND Status != Inactive
Cross-user ID → 404 (not 403)
Early bug: all users saw all todos → fixed + AuthorizationTests
```

---

## Top 3 Resume Stories (STAR — pick by question)

### Story A — Zapier user deletion / compliance
- **Use for:** prod incident, data integrity, compliance, persistence
- **S:** 2000+ user deletion failures blocking SOC2/GDPR/CCPA audits
- **T:** Fix root cause + make deletions reliable at scale
- **A:** Diagnose failure modes → implement improvements → work with compliance stakeholders
- **R:** Audits passed; reliable deletion for identity management across product
- **Ezra tie:** "Member data lifecycle — retention, deletion, auditability — same rigor as PHI"

### Story B — Lighter Capital webhooks
- **Use for:** integration errors, API design, C#/.NET, measurable improvement
- **S:** Third-party API integration had high error rate
- **T:** Redesign integration for reliability
- **A:** Moved from polling to webhooks; AWS SNS/Lambdas/S3 pipeline
- **R:** 60% error reduction
- **Ezra tie:** "Imaging partner integrations need the same event-driven reliability"

### Story C — Carta company merge
- **Use for:** end-to-end ownership, testing, saving manual ops time
- **S:** 3-hour manual merge process, error-prone
- **T:** Automate with full test coverage
- **A:** Designed feature, Postgres/Django/React, edge cases
- **R:** Single click; ~1080 associate hours saved/year
- **Ezra tie:** "Clinical ops automation — fewer manual steps, fewer mistakes"

### Story D — Take-home UserId bug (backup)
- **S:** Early build returned all users' entries
- **A:** Scoped every query to claim; 404 for cross-user; added AuthorizationTests
- **R:** Regression tests; same pattern I'd use for PHI isolation

---


**Health checks:** Endpoints the orchestrator (Kubernetes, Azure App Service) polls.

| Endpoint | Checks | Example response |
|----------|--------|------------------|
| `GET /health/live` | Process up | 200 OK |
| `GET /health/ready` | Can serve traffic? DB reachable? | 200 or 503 |

**Example:** After deploy, K8s calls `/health/ready` every 10s. SQLite connection dead → 503 → pod removed from load balancer until fixed.

**Metrics:** Numbers over time for dashboards/alerts.

| Metric | Example use |
|--------|-------------|
| `http_requests_total{status="500"}` | Alert if 500 rate spikes |
| `http_request_duration_seconds` | p95 latency regression |
| `auth_login_failures_total` | Brute-force signal |
| Custom: `entry_create_total` | Business activity |

**Your app today:** Serilog **logs** requests (text/structured) — good for debugging, not the same as metrics. Production = Serilog → aggregator **plus** Prometheus/Application Insights counters.

**Interview line:** "I have structured logging and a global exception handler; health checks and metrics would be the next ops layer for orchestration and alerting at Ezra scale."

---

## Tradeoffs Quick Reference

| Chose | Over | Because |
|-------|------|---------|
| Cookies + Identity | JWT | Server-side logout; HttpOnly |
| SQLite in-mem | EF InMemory | Real SQL constraints |
| Soft delete | Hard delete | Future trash/restore |
| REST + OpenAPI | GraphQL | Simple CRUD; typed client |
| Forward-only status | Loop to Active | No accidental undo |
| Pop-out window | Resize browser | Reliable side-panel UX |

---

## Likely Questions → Answer in One Breath

| Question | Answer hook |
|----------|-------------|
| Why cookies not JWT? | Real logout; same-origin SPA; Identity built-in |
| 10K todos? | Pagination + index (UserId, Status) + virtualize |
| 3 API instances? | Redis session store |
| Two tabs editing? | Last write wins; BroadcastChannel refetch; ETags later |
| AI for C#? | Boilerplate only; wiped once; small PRs; tests guard |
| Why Ezra? | Mission + clinical empathy + fintech compliance muscle |
| HIPAA experience? | Klick-adjacent via Zapier SOC2/GDPR; same least-privilege mindset |
| Weakness? | In-mem DB, no pagination — documented with scale path |
| SQL Server / Azure? | Postgres + AWS on resume; relational + cloud patterns transfer; .NET from take-home + Lighter |
| EMR / HL7 / DICOM? | No SWE exp; clinical background + integration stories; eager to learn |
| Vue vs React? | Chose React for take-home review bandwidth; ramp Vue same patterns |

---

## Questions for Kristina (Team Lead)

1. What does your pod own day-to-day — Hub, scheduling, or reporter?
2. How do you balance shipping speed with QA/data quality given your background?
3. What would the first 90 days look like for someone joining your pod?

## Questions for Michael (Staff)

1. Where do you see the biggest architectural bets for the imaging platform in the next year?
2. What does a well-run production incident look like on your team?
3. How do staff engineers influence standards without becoming a bottleneck?

---

## Day-Before Checklist

- [x] `dotnet test` — 65 passed, 1 skipped (see [EzraInterviewLiveDemo.md](EzraInterviewLiveDemo.md))
- [x] `pnpm test:ci` — 17 passed
- [ ] `docker compose up --build` — run locally (Docker daemon was not running during prep)
- [ ] Manual UI walkthrough — register, todos, status, pop-out, settings, logout (see LiveDemo checklist)
- [ ] Skim EntryController, useEntries, DESIGN.md §3/§5/§8
- [ ] Rehearse 60-sec opener + Story A out loud
- [ ] Have repo open; know where auth tests live

---

## Mindset

- **Senior =** judgment + honesty + ownership, not perfection
- **Kristina may probe:** data quality, testing, cross-functional delivery, mentoring
- **Michael may probe:** architecture depth, prod debugging, tradeoff defense, scale
- Name gaps before they do · Connect answers to member trust and clinical workflows
