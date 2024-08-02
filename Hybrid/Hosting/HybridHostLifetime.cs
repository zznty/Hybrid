﻿using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.NET.Windowing;

namespace Hybrid.Hosting;

internal class HybridHostLifetime(IEnumerable<IHostedService> hostedServices, ILogger<HybridHostLifetime> logger) : IHostLifetime, IHostApplicationLifetime
{
    private IWindow? _window;
    private readonly SemaphoreSlim _startSemaphore = new(0);
    private readonly SemaphoreSlim _stopSemaphore = new(0);
    private readonly CancellationTokenSource _stoppingTokenSource = new();
    private readonly CancellationTokenSource _startedTokenSource = new();
    
    internal void SetWindow(IWindow window)
    {
        _stoppingTokenSource.TryReset();
        _startedTokenSource.TryReset();
        
        _window = window;
        
        _window.Load += WindowOnLoad;
        _window.Closing += WindowOnClosing;
    }

    private async void WindowOnLoad()
    {
        try
        {
            await DispatchStartAsync(_stoppingTokenSource.Token);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Host initialization failed.");
            await StopAsync(default);
            throw;
        }
        
        _startSemaphore.Release();
        await _startedTokenSource.CancelAsync();
    }

    private async Task DispatchStartAsync(CancellationToken cancellationToken)
    {
        foreach (var hostedService in hostedServices)
        {
            if (hostedService is IHostedLifecycleService beforeStartLifecycle)
                await beforeStartLifecycle.StartingAsync(cancellationToken);

            if (_window is not null && hostedService is IHybridHostedService hybridHostedService)
                await hybridHostedService.StartAsync(_window, cancellationToken);
            else
                await hostedService.StartAsync(cancellationToken);
            
            if (hostedService is IHostedLifecycleService afterStartLifecycle)
                await afterStartLifecycle.StartedAsync(cancellationToken);
        }
    }
    
    private void WindowOnClosing()
    {
        _stoppingTokenSource.Cancel();
        _stopSemaphore.Release();
    }

    public Task WaitForStartAsync(CancellationToken cancellationToken) => _startSemaphore.WaitAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_window is null)
            throw new InvalidOperationException("Host did not finish initialization.");
        
        return _stopSemaphore.WaitAsync(cancellationToken);
    }

    public void StopApplication()
    {
        _window?.Close();
    }

    public CancellationToken ApplicationStarted => _startedTokenSource.Token;
    public CancellationToken ApplicationStopping => _stoppingTokenSource.Token;
    public CancellationToken ApplicationStopped => _stoppingTokenSource.Token;
}