using System.Data;
using System.Data.Common;
using System.Reflection;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class SlowQueryInterceptorBehaviorTests
{
    private sealed class FakeDbCommand(string commandText = "SELECT 1") : DbCommand
    {
        public override string CommandText { get; set; } = commandText;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection? DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
        protected override DbTransaction? DbTransaction { get; set; }
        public override void Cancel() { }
        public override int ExecuteNonQuery() => 0;
        public override object? ExecuteScalar() => null;
        public override void Prepare() { }
        protected override DbParameter CreateDbParameter() => throw new NotSupportedException();
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new NotSupportedException();
    }

    private sealed class FakeDbParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _inner = [];
        public override int Count => _inner.Count;
        public override object SyncRoot => _inner;
        public override int Add(object? value) { _inner.Add((DbParameter)value!); return _inner.Count - 1; }
        public override void AddRange(Array values) { foreach (DbParameter p in values) _inner.Add(p); }
        public override void Clear() => _inner.Clear();
        public override bool Contains(object? value) => _inner.Contains((DbParameter)value!);
        public override bool Contains(string value) => _inner.Any(p => p.ParameterName == value);
        public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_inner).CopyTo(array, index);
        public override System.Collections.IEnumerator GetEnumerator() => _inner.GetEnumerator();
        public override int IndexOf(object? value) => _inner.IndexOf((DbParameter)value!);
        public override int IndexOf(string parameterName) => _inner.FindIndex(p => p.ParameterName == parameterName);
        public override void Insert(int index, object? value) => _inner.Insert(index, (DbParameter)value!);
        public override void Remove(object? value) => _inner.Remove((DbParameter)value!);
        public override void RemoveAt(int index) => _inner.RemoveAt(index);
        public override void RemoveAt(string parameterName) => _inner.RemoveAt(IndexOf(parameterName));
        protected override DbParameter GetParameter(int index) => _inner[index];
        protected override DbParameter GetParameter(string parameterName) => _inner.First(p => p.ParameterName == parameterName);
        protected override void SetParameter(int index, DbParameter value) => _inner[index] = value;
        protected override void SetParameter(string parameterName, DbParameter value) => _inner[IndexOf(parameterName)] = value;
    }

    private static readonly ConstructorInfo CommandExecutedEventDataCtor =
        typeof(CommandExecutedEventData)
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

    private static CommandExecutedEventData CreateEventData(TimeSpan duration)
    {
        var ps = CommandExecutedEventDataCtor.GetParameters();
        var ctorArgs = new object?[ps.Length];
        for (int i = 0; i < ps.Length; i++)
        {
            ctorArgs[i] = (ps[i].Name) switch
            {
                "duration" => duration,
                "startTime" => DateTimeOffset.UtcNow,
                "commandId" => Guid.NewGuid(),
                "connectionId" => Guid.NewGuid(),
                "async" => false,
                "logParameterValues" => false,
                _ => null
            };
        }
        return (CommandExecutedEventData)CommandExecutedEventDataCtor.Invoke(ctorArgs)!;
    }

    private static DbCommand CreateFakeCommand(string commandText = "SELECT 1") =>
        new FakeDbCommand(commandText);

    private static Mock<DbDataReader> CreateFakeReader()
    {
        var mock = new Mock<DbDataReader>();
        mock.Setup(r => r.IsClosed).Returns(false);
        return mock;
    }

    [Fact]
    public void ReaderExecuted_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        var reader = CreateFakeReader().Object;

        interceptor.ReaderExecuted(command, eventData, reader);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void ReaderExecuted_Should_NotInvokeHandler_When_DurationBelowThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromSeconds(5) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(50));
        var reader = CreateFakeReader().Object;

        interceptor.ReaderExecuted(command, eventData, reader);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Never);
    }

    [Fact]
    public void ReaderExecuted_Should_PassCorrectContextToHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        SlowQueryContext? capturedContext = null;
        var handlerMock = new Mock<ISlowQueryHandler>();
        handlerMock.Setup(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()))
            .Callback<SlowQueryContext>(ctx => capturedContext = ctx);
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(10) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand("SELECT * FROM Users");
        var elapsed = TimeSpan.FromMilliseconds(999);
        var eventData = CreateEventData(elapsed);
        var reader = CreateFakeReader().Object;

        interceptor.ReaderExecuted(command, eventData, reader);

        capturedContext.Should().NotBeNull();
        capturedContext!.CommandText.Should().Be("SELECT * FROM Users");
        capturedContext.ElapsedTime.Should().Be(elapsed);
        capturedContext.MethodName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ReaderExecutedAsync_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        var reader = CreateFakeReader().Object;

        await interceptor.ReaderExecutedAsync(command, eventData, reader);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public async Task ReaderExecutedAsync_Should_NotInvokeHandler_When_DurationBelowThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromSeconds(5) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(10));
        var reader = CreateFakeReader().Object;

        await interceptor.ReaderExecutedAsync(command, eventData, reader);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Never);
    }

    [Fact]
    public void ScalarExecuted_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));

        interceptor.ScalarExecuted(command, eventData, result: null);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void ScalarExecuted_Should_NotInvokeHandler_When_DurationBelowThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromSeconds(5) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(10));

        interceptor.ScalarExecuted(command, eventData, result: null);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Never);
    }

    [Fact]
    public async Task ScalarExecutedAsync_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));

        await interceptor.ScalarExecutedAsync(command, eventData, result: null);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void NonQueryExecuted_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));

        interceptor.NonQueryExecuted(command, eventData, result: 1);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void NonQueryExecuted_Should_NotInvokeHandler_When_DurationBelowThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromSeconds(5) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(10));

        interceptor.NonQueryExecuted(command, eventData, result: 1);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Never);
    }

    [Fact]
    public async Task NonQueryExecutedAsync_Should_InvokeHandler_When_DurationExceedsThreshold()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(100) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options, handlerMock.Object);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));

        await interceptor.NonQueryExecutedAsync(command, eventData, result: 1);

        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void HandleIfSlow_Should_UseDefaultThresholdOfOneSecond_When_NoOptionsProvided()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var handlerMock = new Mock<ISlowQueryHandler>();
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, slowQueryHandler: handlerMock.Object);

        var command = CreateFakeCommand();

        interceptor.NonQueryExecuted(command, CreateEventData(TimeSpan.FromMilliseconds(999)), result: 0);
        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Never);

        interceptor.NonQueryExecuted(command, CreateEventData(TimeSpan.FromSeconds(2)), result: 0);
        handlerMock.Verify(h => h.OnSlowQuery(It.IsAny<SlowQueryContext>()), Times.Once);
    }

    [Fact]
    public void HandleIfSlow_Should_NotThrow_When_HandlerIsNull()
    {
        var loggerMock = new Mock<ILogger<SlowQueryInterceptor>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var options = new SlowQueryOptions { Threshold = TimeSpan.FromMilliseconds(10) };
        var interceptor = new SlowQueryInterceptor(loggerMock.Object, options);

        var command = CreateFakeCommand();
        var eventData = CreateEventData(TimeSpan.FromMilliseconds(500));

        Action act = () => interceptor.NonQueryExecuted(command, eventData, result: 0);

        act.Should().NotThrow();
    }

    [Fact]
    public void SlowQueryContext_Should_HaveCorrectProperties_When_Populated()
    {
        var elapsed = TimeSpan.FromMilliseconds(250);
        var ctx = new SlowQueryContext
        {
            CommandText = "SELECT 1",
            Parameters = "p1=1",
            ElapsedTime = elapsed,
            Database = "mydb",
            HasTransaction = true,
            MethodName = "ReaderExecuted"
        };

        ctx.CommandText.Should().Be("SELECT 1");
        ctx.Parameters.Should().Be("p1=1");
        ctx.ElapsedTime.Should().Be(elapsed);
        ctx.Database.Should().Be("mydb");
        ctx.HasTransaction.Should().BeTrue();
        ctx.MethodName.Should().Be("ReaderExecuted");
    }
}
