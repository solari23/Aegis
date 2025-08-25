using Aegis.Passkeys;

namespace WebAuthNPlay
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var pkman = new PasskeyManager();
            pkman.Do();
            //pkman.Do2();
            //pkman.Do3();
        }
    }
}
