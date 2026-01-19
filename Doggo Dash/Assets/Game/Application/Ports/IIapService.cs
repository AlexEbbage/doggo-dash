using System;

namespace Game.Application.Ports
{
    public interface IIapService
    {
        bool IsInitialized { get; }
        bool CanPurchase(string productId);
        void Purchase(string productId, Action<bool> onCompleted);
    }
}
