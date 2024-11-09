namespace EncryptionDecryption;

class Program
{
    static void Main(string[] args)
    {
        var file = File.Open("/home/shashanka/Downloads/VID-20241027-WA0030.mp4", FileMode.Open, FileAccess.Read, FileShare.Read);

        Console.WriteLine(file.Length);
    }
}