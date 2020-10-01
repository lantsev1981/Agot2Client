using GameService;

namespace Agot2Client
{

    public interface IPosition
    {
        WCFGamePoint Position { get; set; }
        void OnPropertyChanged(string propName);
    }
}
