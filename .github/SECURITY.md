# Security Policy

## Supported Versions

We actively support the following versions of DotGnatly with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 0.1.x   | :white_check_mark: |
| < 0.1   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security issue in DotGnatly, please report it responsibly.

### How to Report

**DO NOT** create a public GitHub issue for security vulnerabilities.

Instead, please report security vulnerabilities by:

1. **Email**: Send details to [security@your-domain.com](mailto:security@your-domain.com)
2. **GitHub Security Advisories**: Use the [Security Advisory](https://github.com/your-org/DotGnatly/security/advisories/new) feature

### What to Include

Please include the following information in your report:

- **Description**: Clear description of the vulnerability
- **Impact**: What can an attacker do with this vulnerability?
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Affected Versions**: Which versions are affected?
- **Proof of Concept**: Code snippet or configuration demonstrating the issue
- **Suggested Fix**: If you have ideas on how to fix it (optional)

### Example Report Format

```
Subject: [SECURITY] Vulnerability in NatsController Configuration

Description:
A vulnerability exists in the NatsController that allows...

Impact:
An attacker could potentially...

Steps to Reproduce:
1. Create a NatsController with the following configuration...
2. Call the method...
3. Observe that...

Affected Versions:
- v0.1.0
- v0.1.1

Proof of Concept:
[Code snippet or detailed explanation]

Suggested Fix:
[Your suggestions if any]
```

### Response Timeline

We will acknowledge receipt of your vulnerability report within **48 hours** and will send a more detailed response within **7 days** indicating the next steps in handling your report.

After the initial reply to your report, we will:

- **Investigate**: Confirm and analyze the vulnerability
- **Patch**: Develop a fix for supported versions
- **Coordinate**: Work with you on disclosure timeline
- **Release**: Publish security advisory and patched versions
- **Credit**: Acknowledge your contribution (if desired)

### Disclosure Policy

- **Coordinated Disclosure**: We follow a coordinated disclosure process
- **Timeline**: We aim to release patches within 90 days of report
- **Communication**: We'll keep you informed throughout the process
- **Credit**: We'll credit reporters in security advisories (unless you prefer anonymity)

### Security Best Practices

When using DotGnatly in production:

1. **Authentication**: Always enable authentication in production environments
2. **TLS**: Use TLS for encrypted connections
3. **Port Configuration**: Don't expose NATS ports directly to the internet
4. **Updates**: Keep DotGnatly and NATS server updated to the latest versions
5. **Access Control**: Use proper access control and authorization
6. **Monitoring**: Monitor for suspicious activity using built-in monitoring endpoints
7. **Validation**: Always validate configuration before applying changes
8. **Secrets**: Never commit credentials or sensitive configuration to source control

### Security Features in DotGnatly

DotGnatly includes several security features:

- **Pre-Apply Validation**: Prevents invalid or unsafe configurations
- **Configuration Versioning**: Allows rollback if issues are detected
- **Event System**: Monitor configuration changes in real-time
- **TLS Support**: Secure client-server communication
- **Authentication**: Support for user/password and token authentication
- **Account Isolation**: Multi-tenancy with account-level isolation

### Dependencies

DotGnatly depends on:

- **NATS Server**: We track security advisories from the NATS project
- **.NET Runtime**: Follow .NET security guidelines
- **Native Bindings**: Go runtime security considerations

We regularly review and update dependencies to address known vulnerabilities.

### Security Scanning

Our CI/CD pipeline includes:

- **CodeQL Analysis**: Automated security scanning on every PR
- **Dependabot**: Automated dependency updates with security patches
- **Manual Reviews**: Security-focused code reviews for sensitive changes

### Contact

- **Security Email**: security@your-domain.com
- **PGP Key**: [Link to PGP public key if available]
- **Security Advisories**: https://github.com/your-org/DotGnatly/security/advisories

### Hall of Fame

We maintain a hall of fame for security researchers who responsibly disclose vulnerabilities:

<!-- This section will be updated as researchers report issues -->

---

Thank you for helping keep DotGnatly and our users safe!
