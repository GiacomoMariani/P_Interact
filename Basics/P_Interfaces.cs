//the interfaces for playfab interact
using JReact.Playfab_Interact.Data;

namespace JReact.Playfab_Interact
{
    public interface iDataConvertable<out T>
        where T : SaveableData
    {
        T ConvertToData();
    }
    
    public interface iDataReceiver<in T>
        where T : SaveableData
    {
        void ReceiveData(T dataReceived);
    }
}
