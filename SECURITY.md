# Security Policy

## Supported Versions

Starting from 2026, only JEngine version 1.x receives security updates. Earlier versions (0.x) are no longer maintained and users are strongly encouraged to upgrade.

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security issue, please report it responsibly.

### How to Report

**Please DO NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via one of the following methods:

1. **GitHub Security Advisories** (Preferred)
   - Go to the [Security Advisories](https://github.com/JasonXuDeveloper/JEngine/security/advisories) page
   - Click "Report a vulnerability"
   - Provide detailed information about the vulnerability

2. **Email**
   - Send an email to: jason@xgamedev.net
   - Use subject line: `[SECURITY] JEngine Vulnerability Report`
   - Include as much detail as possible

### What to Include

When reporting a vulnerability, please include:

- Type of vulnerability (e.g., code injection, XSS, authentication bypass)
- Location of the affected source code (file path, line numbers)
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if available)
- Potential impact of the vulnerability
- Any suggested fixes (optional)

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your report within 48 hours
- **Initial Assessment**: We will provide an initial assessment within 7 days
- **Resolution Timeline**: We aim to resolve critical vulnerabilities within 30 days
- **Credit**: If you wish, we will credit you in the security advisory and release notes

### Safe Harbor

We consider security research conducted in accordance with this policy to be:

- Authorized and we will not pursue legal action
- Conducted in good faith
- Helpful to the security of our users

## Security Best Practices for JEngine Users

### Encryption

JEngine supports multiple encryption algorithms for hot update bundles:

- **XOR** - Fast but basic; suitable for non-sensitive content
- **AES** - Recommended for most use cases
- **ChaCha20** - Recommended for high-security requirements

Always use AES or ChaCha20 for production deployments.

### Code Signing

When deploying hot updates, ensure your build pipeline:

1. Uses secure, private signing keys
2. Validates signatures before loading updates
3. Stores keys securely (not in source control)

### Network Security

- Always use HTTPS for CDN endpoints serving hot update bundles
- Implement certificate pinning where possible
- Validate all downloaded content before execution

## Dependencies

JEngine relies on the following third-party packages. Keep them updated:

| Package | Security Notes |
|---------|----------------|
| HybridCLR | Runtime code execution - keep updated |
| YooAsset | Asset loading - verify bundle integrity |
| Nino | Serialization - validate input data |

## Version History

| Date | Version | Security Changes |
|------|---------|------------------|
| 2026 | 1.0.0+  | Initial security policy established |
