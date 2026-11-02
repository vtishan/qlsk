using System.Text;

namespace QLSK.Setup;

internal static class SetupConsole
{
    public static string? ChooseCommand()
    {
        Console.WriteLine("Thiết lập QLSK");
        Console.WriteLine("  1. Tạo admin mới");
        Console.WriteLine("  2. Đổi email/mật khẩu admin");
        Console.WriteLine("  3. Cập nhật cấu trúc database (migrate)");
        Console.WriteLine("  0. Thoát");
        while (true)
        {
            Console.Write("Chọn [0-3]: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": return "create-admin";
                case "2": return "update-admin";
                case "3": return "migrate";
                case "0":
                case null: return null;
                default: Console.WriteLine("Vui lòng chọn 0, 1, 2 hoặc 3."); break;
            }
        }
    }

    public static string ReadRequired(string label)
    {
        Console.Write($"{label}: ");
        var value = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new SetupValidationException($"Chưa nhập {label.ToLowerInvariant()}.");
        return value;
    }

    public static string ReadNewPassword()
    {
        Console.WriteLine("Mật khẩu: ít nhất 12 ký tự, gồm ít nhất 3 nhóm chữ hoa, chữ thường, số, ký tự đặc biệt.");
        var password = ReadPassword("Mật khẩu mới");
        if (password != ReadPassword("Nhập lại mật khẩu"))
            throw new SetupValidationException("Hai mật khẩu không khớp. Chưa thay đổi tài khoản.");
        return password;
    }

    private static string ReadPassword(string label)
    {
        Console.Write($"{label}: ");
        var value = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return value.ToString();
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (value.Length > 0) value.Length--;
            }
            else if (!char.IsControl(key.KeyChar)) value.Append(key.KeyChar);
        }
    }
}
