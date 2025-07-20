using AppShoping.UI.Menu;

namespace AppShoping.UI.App;

public class App : IApp
{
    private readonly IUserCommunication _menu;

    public App(IUserCommunication menu)
    {
        _menu = menu ?? throw new ArgumentNullException(nameof(menu));
    }

    public void Run()
    {
        try
        {
            _menu.DisplayMenu();
            _menu.ChoiceOfMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
    }
}
