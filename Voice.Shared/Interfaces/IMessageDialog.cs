
namespace Voice.Shared.Core.Interfaces
{
    public interface IMessageDialog
    {
    
        void ErrorMessage(string message, string title = null);
        void ErrorMessage(string key, string message, string controller_key=null);
         
        
    }
}
