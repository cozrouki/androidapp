public class udpInputMessage
{
    public string GameName { get; private set; }
    public int PlayerIndex { get; private set; } // 1~4
    public string KeyCode { get; private set; }

    public bool IsValid { get; private set; }

    public static udpInputMessage Parse(string raw)
    {
        var result = new udpInputMessage();

        if (string.IsNullOrEmpty(raw))
        {
            result.IsValid = false;
            return result;
        }

        string[] tokens = raw.Split(':');
        if (tokens.Length != 3)
        {
            // "soft_reset"과 같은 단일 명령 처리
            result.KeyCode = raw.Trim();
            result.IsValid = !string.IsNullOrEmpty(result.KeyCode);
            return result;
        }

        result.GameName = tokens[0].Trim();
        result.KeyCode = tokens[2].Trim();

        if (!int.TryParse(tokens[1], out int p))
        {
            result.IsValid = false;
            return result;
        }

        if (p < 1 || p > 4)
        {
            result.IsValid = false;
            return result;
        }

        result.PlayerIndex = p;
        result.IsValid = true;

        return result;
    }
}
