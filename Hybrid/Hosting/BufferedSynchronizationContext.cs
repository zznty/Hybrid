using System.Threading.Channels;

namespace Hybrid.Hosting;

public sealed class BufferedSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly Channel<DispatchItem> _channel;
    private readonly ChannelReader<DispatchItem>? _reader;
    private readonly ChannelWriter<DispatchItem> _writer;

    public BufferedSynchronizationContext() : this(Channel.CreateUnbounded<DispatchItem>(new()
    {
        SingleReader = true
    }))
    {
        _reader = _channel;
    }

    private BufferedSynchronizationContext(Channel<DispatchItem> channel)
    {
        _channel = channel;
        _writer = channel;
    }

    public void Dispatch()
    {
        if (_reader is null)
            return;
        
        while (_reader.TryRead(out var item))
        {
            try
            {
                item.Callback(item.State);
            }
            catch (Exception e)
            {
                var ex = new AggregateException("Dispatching callback failed.", e);
                _writer.Complete(ex);
                throw ex;
            }
            item.CompletionEvent?.Set();
        }
    }
    
    public override void Post(SendOrPostCallback d, object? state)
    {
        if (!_writer.TryWrite(new(d, state)))
            throw new InvalidOperationException("Could not write to the channel.");
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        var completionEvent = new ManualResetEventSlim();
        
        if (!_writer.TryWrite(new(d, state, completionEvent)))
            throw new InvalidOperationException("Could not write to the channel.");
        
        completionEvent.Wait();
    }

    public override SynchronizationContext CreateCopy() => new BufferedSynchronizationContext(_channel);

    private readonly record struct DispatchItem(
        SendOrPostCallback Callback,
        object? State,
        ManualResetEventSlim? CompletionEvent = null);

    public void Dispose()
    {
        _writer.Complete();
    }
}