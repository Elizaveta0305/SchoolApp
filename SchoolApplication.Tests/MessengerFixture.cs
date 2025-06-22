using CommunityToolkit.Mvvm.Messaging;
using Xunit;

[CollectionDefinition("MessengerCollection")]
public class MessengerCollection : ICollectionFixture<MessengerFixture>
{
  
}

public class MessengerFixture : IDisposable
{
    public IMessenger Messenger { get; }

    public MessengerFixture()
    {
        Messenger = WeakReferenceMessenger.Default;
        Messenger.Reset();
    }

    public void Dispose()
    {
        Messenger.Reset();
    }
}