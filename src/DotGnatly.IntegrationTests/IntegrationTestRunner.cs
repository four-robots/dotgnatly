namespace DotGnatly.IntegrationTests;

/// <summary>
/// Runs all integration tests and collects results.
/// </summary>
public class IntegrationTestRunner
{
    private readonly List<IIntegrationTest> _tests = new();

    public IntegrationTestRunner()
    {
        // Register all test suites
        _tests.Add(new MultiServerTests());
        _tests.Add(new LeafNodeConfigurationTests());
        _tests.Add(new HubAndSpokeTests());
        _tests.Add(new ValidationTests());
        _tests.Add(new EventSystemTests());
        _tests.Add(new ConcurrentOperationTests());
        _tests.Add(new ConfigurationReloadTests());
        _tests.Add(new MonitoringTestSuite());
        _tests.Add(new AccountManagementTestSuite());
        _tests.Add(new AuthorizationAndFilteringTestSuite());
        _tests.Add(new RuntimeControlTestSuite());
        _tests.Add(new LoggingAndClusteringTests());
    }

    public async Task<TestResults> RunAllTestsAsync(bool verbose = false)
    {
        var results = new TestResults(verbose);

        if (verbose)
        {
            Console.WriteLine($"Running {_tests.Count} test suites...");
            Console.WriteLine();
        }

        for (int i = 0; i < _tests.Count; i++)
        {
            var test = _tests[i];

            if (verbose)
            {
                Console.WriteLine($"[{i + 1}/{_tests.Count}] Running {test.GetType().Name}...");
                Console.WriteLine(new string('-', 60));
            }

            try
            {
                await test.RunAsync(results);

                if (verbose)
                {
                    Console.WriteLine($"✓ {test.GetType().Name} completed");
                }
            }
            catch (Exception ex)
            {
                // Always show exceptions
                Console.WriteLine($"✗ {test.GetType().Name} failed with exception: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                }
                results.AddFailure($"{test.GetType().Name}: {ex.Message}");
            }

            if (verbose)
            {
                Console.WriteLine();
            }
        }

        return results;
    }
}

/// <summary>
/// Interface for integration test suites.
/// </summary>
public interface IIntegrationTest
{
    Task RunAsync(TestResults results);
}

/// <summary>
/// Tracks test results.
/// </summary>
public class TestResults
{
    private int _totalTests = 0;
    private int _passedTests = 0;
    private readonly List<string> _failures = new();
    private readonly bool _verbose;

    public TestResults(bool verbose = false)
    {
        _verbose = verbose;
    }

    public int TotalTests => _totalTests;
    public int PassedTests => _passedTests;
    public int FailedTests => _totalTests - _passedTests;
    public double SuccessRate => _totalTests == 0 ? 0 : (_passedTests / (double)_totalTests) * 100;
    public IReadOnlyList<string> Failures => _failures;
    public bool IsVerbose => _verbose;

    public void StartTest(string testName)
    {
        if (_verbose)
        {
            Console.WriteLine($"  → Starting: {testName}");
        }
    }

    public void RecordTest(string testName, bool passed, string? errorMessage = null)
    {
        _totalTests++;

        if (passed)
        {
            _passedTests++;
            if (_verbose)
            {
                Console.WriteLine($"  ✓ {testName}");
            }
        }
        else
        {
            // Always show failed tests
            Console.WriteLine($"  ✗ {testName}");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine($"    Error: {errorMessage}");
                _failures.Add($"{testName}: {errorMessage}");
            }
            else
            {
                _failures.Add(testName);
            }
        }
    }

    public void AddFailure(string message)
    {
        _failures.Add(message);
    }

    public async Task AssertAsync(string testName, Func<Task<bool>> testFunc)
    {
        StartTest(testName);
        try
        {
            bool result;
            if (_verbose)
            {
                result = await testFunc();
            }
            else
            {
                using (SuppressConsoleOutput())
                {
                    result = await testFunc();
                }
            }
            RecordTest(testName, result, result ? null : "Test returned false");
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, ex.Message);
        }
    }

    public async Task AssertNoExceptionAsync(string testName, Func<Task> testFunc)
    {
        StartTest(testName);
        try
        {
            if (_verbose)
            {
                await testFunc();
            }
            else
            {
                using (SuppressConsoleOutput())
                {
                    await testFunc();
                }
            }
            RecordTest(testName, true);
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, ex.Message);
        }
    }

    public async Task AssertExceptionAsync<TException>(string testName, Func<Task> testFunc) where TException : Exception
    {
        StartTest(testName);
        try
        {
            if (_verbose)
            {
                await testFunc();
            }
            else
            {
                using (SuppressConsoleOutput())
                {
                    await testFunc();
                }
            }
            RecordTest(testName, false, $"Expected {typeof(TException).Name} but no exception was thrown");
        }
        catch (TException)
        {
            RecordTest(testName, true);
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
        }
    }

    private IDisposable SuppressConsoleOutput()
    {
        return new ConsoleOutputSuppressor();
    }

    private class ConsoleOutputSuppressor : IDisposable
    {
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalError;

        public ConsoleOutputSuppressor()
        {
            _originalOut = Console.Out;
            _originalError = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOut);
            Console.SetError(_originalError);
        }
    }
}
