using System.Runtime.InteropServices;
using System.Security;


public static class PasswordInput
{
    private static string possibleCharacters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
    private static SecureString _password { get; } = new SecureString();


    public static void SetSecurePassword()
    {
        Console.Write(".. ");
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Enter)
            {
                if (key.Key != ConsoleKey.Backspace)
                {
                    _password.AppendChar(key.KeyChar);
                }
                else if (_password.Length > 0)
                {
                    _password.RemoveAt(_password.Length - 1);
                }
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        _password.MakeReadOnly();
    }

    public static SecureString ConvertString(string password) 
    {
        SecureString testablePassword = new SecureString();

        try 
        {
            for (int i = 0; i < password.Length; i++)
            {
                testablePassword.AppendChar(password[i]);
            }
        }
        catch 
        {
            Random random = new Random();

            for (int i = 0; i < random.Next(16, 32); i++)
            {
                testablePassword.AppendChar(possibleCharacters[random.Next(0, possibleCharacters.Length)]);
            }
        }

        testablePassword.MakeReadOnly();
        return testablePassword;
    }

    public static bool SecureEquals(SecureString passwordToTest)
    {
        if (passwordToTest == null || _password == null) 
        {
            return false;
        }

        if (passwordToTest.Length != _password.Length) 
        {
            return false;
        }

        IntPtr ptrA = IntPtr.Zero;
        IntPtr ptrB = IntPtr.Zero;

        try
        {
            ptrA = Marshal.SecureStringToGlobalAllocUnicode(passwordToTest);
            ptrB = Marshal.SecureStringToGlobalAllocUnicode(_password);

            unsafe
            {
                char* pA = (char*)ptrA;
                char* pB = (char*)ptrB;

                for (int i = 0; i < passwordToTest.Length; i++)
                {
                    if (pA[i] != pB[i]) 
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        finally
        {
            if (ptrA != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(ptrA);

            if (ptrB != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(ptrB);

            passwordToTest.Dispose();
        }
    }
}