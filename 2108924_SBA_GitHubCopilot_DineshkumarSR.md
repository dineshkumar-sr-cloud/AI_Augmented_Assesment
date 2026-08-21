# 2108924_SBA_GitHubCopilot_DineshkumarSR

This markdown file is a ready-to-fill assembly of the Word document required by the assessment. Download and convert to .docx (or copy/paste into Word) and insert screenshots into the placeholders specified.

Filename when converted: 2108924_SBA_GitHubCopilot_DineshkumarSR.docx

---

## Title page

SBA GitHub Copilot Assessment  
Associate ID: 2108924  
Name: DineshkumarSR  
Date: <YYYY-MM-DD>

---

SECTION 1: Setup & Project Standards (Page 1–2)

Screenshot 1.1 — IDE with Copilot Active

[Insert Screenshot_1_1_IDE_Copilot.png here]

Screenshot 1.2 — .github/copilot-instructions.md Content

[Insert Screenshot_1_2_copilot-instructions.png here]

If the file scrolls, add additional screenshots named Screenshot_1_2_part2.png, etc.

Write:

1A. Rationale (2–3 sentences)

[Type your 2–3 sentence rationale here]

Suggested example to adapt:

> I structured copilot-instructions.md to enforce multi-tenant safety, consistent coding standards, and testing requirements. The most important rules are organizationId scoping, explicit error handling, and testing expectations — these prevent data leakage and ensure auditability and traceability, which are critical in fintech.

---

SECTION 2: Transaction Module — AI Generation (Page 3–4)

What to do:
- Open Copilot Chat and enter exactly: "Generate a Transaction model and a Transaction service with create, get-by-user, and delete-all functions. Use a database."  
- Let Copilot generate and save the generated files (do not modify before screenshot).

Screenshot 2.1 — The Low-Effort Prompt in Copilot Chat

[Insert Screenshot_2_1_CopilotChat_Prompt.png here]

Screenshot 2.2 — Generated Files

[Insert Screenshot_2_2_GeneratedModel.png here]

[Insert Screenshot_2_2_GeneratedService.png here]

Write:

2A. Copilot Chat mode used and first impression

Mode used: [Ask / Edit / Agent]  
First impression: [short note — e.g., “The generated code provided CRUD skeletons quickly but used float for currency, lacked validation, and omitted tenant checks.”]

---

SECTION 3: Code Review of AI Output (Page 5–8)

What to do:
- Use Copilot to help review the generated Transaction code, and apply your own expert review. Capture evidence of Copilot prompts / responses and your manual findings.

Screenshot 3.1 — Using Copilot for Review (2–3 screenshots)

[Insert Screenshot_3_1_CopilotReview1.png here]

[Insert Screenshot_3_1_CopilotReview2.png here]

Screenshot 3.2 — An Issue You Caught That Copilot Missed

[Insert Screenshot_3_2_ManualIssue.png here]

Write:

Review Findings Table (minimum 8 rows)

Create a table with columns:
- #
- Location (file + function)
- Category (Bug / Security / Architecture / Performance / Standards)
- Severity (Critical / High / Medium / Low)
- What's Wrong & Fintech Impact
- How I Detected It (Copilot feature used OR "Manual review")
- Recommended Fix

Example rows to adapt (be sure to replace with your actual findings):

1 | TransactionService.CreateAsync | Security | Critical | Uses float for currency → rounding errors and financial loss risk | Manual review | Use decimal with precision and currency code

2 | TransactionRepository.GetByUser | Architecture | High | Missing organizationId filter → cross-tenant data leak | Ask Mode — prompted Copilot to review queries; manual verification | Add organizationId parameter and WHERE clause

3 | TransactionModel.Amount | Standards | High | No precision/column type specified | /explain on model, manual review | Add [Column(TypeName = "decimal(18,2)")] and use decimal

4 | TransactionService.DeleteAll | Performance | Medium | Unbounded delete — could lock table | Manual review | Use batched deletes or background job

5 | TransactionService.CreateAsync | Security | Critical | No input validation or authorization checks | Ask Mode flagged missing auth in chat | Add DTO validation and auth enforcement

6 | TransactionRepository.RawQuery | Security | High | String concatenation — SQL injection risk | /explain on query construction | Use parameterized queries or EF LINQ

7 | GeneratedTests | Standards | Medium | Tests depend on real DB | Manual review | Use mocks or Testcontainers

8 | Logging | Standards | Low | No structured ILogger usage | Manual review | Add ILogger with structured messages

Write:

3A. Issues Copilot Introduced That Required Human Judgment

List at least 2 issues with explanation (why human must catch it and why AI may miss it). Example:

- Currency precision and type: Copilot used float for amounts; humans must enforce decimal and currency units — AI lacks domain-specific financial correctness assumptions.

- Missing tenant scoping: Copilot omitted organizationId in queries; this is a security policy enforced across the codebase and requires human review to ensure system-wide compliance.

---

SECTION 4: Transaction Module — Remediation (Page 9–12)

What to do:
- Rewrite the Transaction module to production standards using Copilot; implement layered architecture (Model → Repository → Service → Controller), use EF Core (or your ORM), add validation, logging, authorization, and exceptions.

Screenshot 4.1 — Remediation Using Copilot (2–3 screenshots)

[Insert Screenshot_4_1_Remediation1.png here]

[Insert Screenshot_4_1_Remediation2.png here]

Screenshot 4.2 — Remediated Code (Key Files)

[Insert Screenshot_4_2_RemediatedModel.png here]

[Insert Screenshot_4_2_RemediatedRepository.png here]

[Insert Screenshot_4_2_RemediatedService.png here]

Write:

4A. Top 3 changes and Copilot mode preference

- Change 1: Replace float with decimal(18,2) and add currency field and validation.
- Change 2: Enforce organizationId across model, repository, and service; validate in middleware.
- Change 3: Add DTO validation, structured ILogger logging, and custom exceptions.

Most useful Copilot mode: [Edit / Agent / Ask] — explain why.

---

SECTION 5: Expense Splitting Feature — New Build (Page 13–18)

What to do:
- Build Expense Splitting feature using Copilot. Generate: Shared Expense Model, Balance Calculation Service, API Endpoints, Tests (≥6).

Screenshot 5.1 — Prompt Chain in Copilot Chat (minimum 4 prompts)

[Insert Screenshot_5_1_PromptChain1.png here]

[Insert Screenshot_5_1_PromptChain2.png here]

[Insert Screenshot_5_1_PromptChain3.png here]

[Insert Screenshot_5_1_PromptChain4.png here]

Screenshot 5.2 — Different Copilot Modes in Action

[Insert Screenshot_5_2_ModeAgent.png here]

[Insert Screenshot_5_2_ModeEdit.png here]

Screenshot 5.3 — Generated Expense Splitting Code

[Insert Screenshot_5_3_ExpenseModel.png here]

[Insert Screenshot_5_3_BalanceService.png here]

[Insert Screenshot_5_3_APIEndpoints.png here]

Screenshot 5.4 — Tests (≥6 cases)

[Insert Screenshot_5_4_Tests.png here]

Write:

Prompt engineering table — include at least 4 prompts with columns:
- Prompt #
- Prompt Text (exact or summarised)
- Copilot Mode Used (Ask/Edit/Agent)
- Other Copilot Features Used (#file, @workspace, /tests, /doc, etc.)
- Prompting Technique Applied (Specificity, Decomposition, Few-shot, Constraint, Role-based, Iterative Refinement)
- Why This Approach?

Write:

5A. Post-Generation Corrections — For each correction list: what Copilot produced, what was wrong, and how you fixed it.

---

SECTION 6: Collaboration & PR Readiness (Page 19–21)

Write — PR Description (3–5 sentences summary + AI disclosure)

PR Description (example)

> Summary: Implemented Transaction and Expense Splitting modules with production-grade patterns: layered architecture, tenant enforcement, and tests. Added SPEC.md, ARCHITECTURE.md, and documentation for reviewers. The changes standardize how audit and notification events are emitted and consumed.

AI Tool Disclosure (example)

- Copilot features used: Ask Mode, Edit Mode, Agent Mode, /explain, /fix, /tests, /doc, #file, @workspace, inline suggestions, copilot-instructions.md.
- Most used mode: Edit Mode for targeted fixes; Agent Mode for scaffolding multi-file features.
- Acceptance vs override: ~60% AI-generated scaffold; ~40% human edits and validation.
- Did copilot-instructions.md help? Yes — it guided consistent naming, organizationId scoping, and testing expectations.

Testing

- Unit tests: cover service methods and business rules (include list of test files)
- Integration tests: basic end-to-end scaffolds using Testcontainers (if present)
- Known gaps: load testing and full DLQ handling not covered in tests

Risks & Trade-offs

- Using async eventing for notifications improves scalability but introduces eventual consistency between services.

Self-Review Checklist

- [ ] No hardcoded secrets or PII
- [ ] All inputs validated
- [ ] Error handling uses custom exceptions
- [ ] Code follows copilot-instructions.md
- [ ] Copilot suggestions reviewed
- [ ] Tests cover happy path, edge cases, error scenarios
- [ ] Used /explain for code I didn't fully understand

Peer Review Simulation — 3 comments (example)

1 | src/Expense/BalanceService.cs : CalculateBalances() | Edge-case: handle zero-amount shares and rounding remainders | Prevents imbalance due to rounding and ensures ledger correctness

2 | src/Transaction/TransactionRepository.cs : GetByUser() | Add organizationId filtering and index on (OrganizationId, UserId) | Prevents cross-tenant data leakage and improves query performance

3 | src/Notifications/DeliveryWorker.cs : EnqueueNotification() | Add idempotency-key handling and DLQ metrics | Ensures at-least-once delivery but prevents duplicate notifications in downstream systems

6A. AI blind spot explanation (2–3 sentences)

> The rounding/ledger balancing edge-case is typically missed by AI because it requires domain-specific rules (financial rounding, business policy for remainders). Human reviewers know requirements for how to allocate fractional cents and ensure ledger totals match, which is outside general coding heuristics AI uses.

---

SECTION 7: Tool Strategy & Reflection (Page 22–25)

Feature Usage Log (min 6 entries) — table columns:
- # | Where in case study | Copilot feature used | Why this feature | Outcome

Example entries to adapt:
1 | Transaction scaffolding | Agent Mode | Scaffold multi-file module quickly | Generated model+service; required domain fixes
2 | Validation fixes | Edit Mode | Targeted diffs with contextual edit | Replaced float with decimal and added validation
3 | Security review | Ask Mode + /explain | Ask for security review of queries | Detected missing tenant scoping
4 | Tests generation | /tests | Produce xUnit test skeletons | Created tests that required DI setup
5 | Documentation | /doc | Generate method docs and spec snippets | Drafted SPEC.md and ARCHITECTURE.md
6 | Commit messages | Copilot generated commit message suggestion | Standardized commit style | Used with human edits

Scenario Responses — for each scenario list which Copilot feature you'd use and why (2–3 sentences each).

Limitations Encountered — provide 3 real examples using the table format from the spec (prompt, output, detection, fix, next time)

---

SECTION 8: Architecture Documentation (Page 26)

Write (10–15 lines):

Transaction and Expense Splitting modules relate as separate domain components where Expense uses Transaction data to compute member balances and settle obligations. The layered architecture is: Route/Controller → Service (business rules & tenant checks) → Repository (EF Core) → Model/DB → Events to Notification & Audit Service. Requests pass tenant context (organizationId) from authentication middleware through services to repositories; services emit idempotent events for downstream processing. This separation supports clear ownership, testability, and independent scaling — important in fintech for audit trails, compliance, and tenant isolation. Key design decisions: use decimal(18,2) for money, enforce organizationId at all layers, prefer async eventing for notifications (trade-off: eventual consistency vs resilience), and centralize audit/event handling to ensure immutable, queryable trails.

---

Screenshot Quality Checklist (copy/paste into the final doc to verify before submission)

- Screenshots are readable and not blurry
- IDE text is legible
- Copilot mode indicators visible where required
- Full prompts visible
- No personal API keys or sensitive information visible
- Screenshots embedded in document (not attached separately)

---

Conversion tip

To convert the Markdown to DOCX with Pandoc:

```bash
pandoc -o 2108924_SBA_GitHubCopilot_DineshkumarSR.docx 2108924_SBA_GitHubCopilot_DineshkumarSR.md
```

Or copy/paste sections into a new Word document and insert screenshots into each placeholder.

---

If you want, I will now:
- (1) Commit this Markdown file to the repository root (I will use the filename `2108924_SBA_GitHubCopilot_DineshkumarSR.md`).
- (2) Also add a short README.md entry explaining how to convert the file to .docx.

You already asked to commit to main; I will proceed now if you confirm.
