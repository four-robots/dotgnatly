# Outdated References in Documentation

⚠️ **NOTE**: This file documents outdated references found in the documentation that need to be updated.

## Summary

The documentation contains references to old class names and namespaces that don't match the current codebase.

### Incorrect References Found:
- **Namespace**: `using NatsSharp;` → Should be `using DotGnatly.Nats;`
- **Class Name**: `NatsServer` → Should be `NatsController`
- **Methods**: Old API methods that may not exist
- **Configuration Classes**: May reference old class names

## Files Requiring Updates

### High Priority (Many References)

1. **API_DESIGN.md**
   - Contains `using NatsSharp;` references
   - References to `NatsServer` class (61+ occurrences across all docs)
   - Update all code examples to use `NatsController`

2. **GETTING_STARTED.md**
   - Multiple `using NatsSharp;` references
   - Code examples use old API
   - Scenario examples need updating

3. **QUICK_REFERENCE.md**
   - Quick start code uses `NatsSharp`
   - Common patterns reference old classes

4. **ARCHITECTURE.md**
   - Diagrams may reference old component names
   - Code examples in architecture explanation

### Lower Priority

5. **DEV_MONITORING.md**
   - May reference old monitoring methods
   - Code examples to check

## Correct Usage (Current Codebase)

### Namespace
```csharp
using DotGnatly.Nats;
using DotGnatly.Core;
```

### Basic Usage
```csharp
// Correct - Current API
using var controller = new NatsController();
var config = new BrokerConfiguration { Port = 4222 };
await controller.ConfigureAsync(config);

// Incorrect - Old API (DO NOT USE)
// using var server = new NatsServer();
// server.Start(new ServerConfig { Port = 4222 });
```

### Configuration Classes
- `BrokerConfiguration` (current)
- ~~`ServerConfig`~~ (old)

### Controller Methods
Current API methods:
- `ConfigureAsync(BrokerConfiguration config)`
- `ApplyChangesAsync(Action<BrokerConfiguration> configure)`
- `GetInfoAsync()`
- `ShutdownAsync()`
- `CreateAccountAsync(AccountConfiguration config)`

Old API (do not use):
- ~~`Start(ServerConfig config)`~~
- ~~`UpdateConfig(ServerConfig config)`~~
- ~~`GetInfo()`~~ (synchronous version)

## Recommended Actions

### Option 1: Mass Update (Recommended)
Use find-and-replace to update all documentation:

```bash
# Update namespace
find docs/ -name "*.md" -exec sed -i 's/using NatsSharp;/using DotGnatly.Nats;/g' {} \;

# Update class name
find docs/ -name "*.md" -exec sed -i 's/NatsServer/NatsController/g' {} \;
find docs/ -name "*.md" -exec sed -i 's/ServerConfig/BrokerConfiguration/g' {} \;

# Update method calls
find docs/ -name "*.md" -exec sed -i 's/\.Start(/\.ConfigureAsync(/g' {} \;
find docs/ -name "*.md" -exec sed -i 's/\.UpdateConfig(/\.ApplyChangesAsync(/g' {} \;
```

⚠️ **Warning**: Review changes after mass update to ensure correctness.

### Option 2: Manual Update
Manually update each file, ensuring code examples match actual API.

### Option 3: Mark as Outdated
Add warning to each affected doc:

> ⚠️ **Note**: Some code examples in this document use outdated API. See [OUTDATED_REFERENCES.md](./OUTDATED_REFERENCES.md) for current API usage.

## Testing Updated Documentation

After updates, verify:
1. All code examples compile
2. All referenced classes exist in codebase
3. All method signatures match actual API
4. All namespaces are correct

## Progress Tracking

- [x] Identified outdated references
- [x] Created this reference document
- [ ] Update API_DESIGN.md
- [ ] Update GETTING_STARTED.md
- [ ] Update QUICK_REFERENCE.md
- [ ] Update ARCHITECTURE.md
- [ ] Update DEV_MONITORING.md
- [ ] Test all code examples
- [ ] Remove this file once complete

---

**Created**: 2025-11-16
**Status**: Documentation consolidation in progress
