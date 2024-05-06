using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class DyScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public Transform[] bpos;
    public List<KMSelectable> buttons;
    public TextMesh[] disp;

    private List<int> enc = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private int[] ops = new int[] { 0, 2, 4, 6 };
    private readonly string[] olog = new string[] { "+", "-", "\u00d7", "\u00f7", "Reversed digits of sum", "Sum of reversed digits", "Reversed digits of difference", "Difference between reversed digits", "First five digits of product", "Last five digits of product", "Remainder prepended to quotient", "Remainder appended to quotient" };
    private readonly int[,] calc = new int[36, 3] { { 705, 1, 64}, { 38, 0, 92}, { 560, 3, 14}, { 435, 0, 51}, { 67, 2, 12}, { 595, 1, 331}, { 7203, 3, 21}, { 26, 0, 27}, { 98, 2, 10}, { 3630, 3, 55}, { 987, 1, 784}, { 12, 2, 18}, { 682, 1, 40}, { 445, 0, 295}, { 909, 3, 101}, { 254, 0, 77}, { 60, 2, 130}, { 414, 1, 255}, { 822, 0, 29}, { 76, 2, 49}, { 51, 3, 17}, { 125, 1, 10}, { 355, 3, 71}, { 66, 0, 289}, { 727, 1, 343}, { 90, 2, 144}, { 8989, 3, 101}, { 212, 2, 39}, { 7997, 0, 11}, { 74, 2, 87}, { 903, 1, 667}, { 551, 3, 19}, { 96, 0, 62}, { 580, 3, 29}, { 604, 1, 64}, { 82, 2, 20} };
    private bool[] entry = new bool[13] { true, true, true, true, true, true, true, true, true, true, false, false, false};
    private int[] nums = new int[3];
    private int ans;
    private bool operand;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private int Errorcrypt(int x, bool dec)
    {
        int y = 0;
        string s = x.ToString();
        for(int i = 0; i < s.Length; i++)
        {
            y *= 10;
            y += dec ? enc.IndexOf(s[i] - '0') : enc[s[i] - '0'];
        }
        return y;
    }

    private int Calc(int a, int o, int b)
    {
        int t = 0;
        if (a < b)
        {
            t = a;
            a = b;
            b = t;
        }
        switch (o)
        {
            case 0:
                t = a + b;
                t = int.Parse(new string(t.ToString().Reverse().ToArray()));
                break;
            case 1:
                a = int.Parse(new string(a.ToString().Reverse().ToArray()));
                b = int.Parse(new string(b.ToString().Reverse().ToArray()));
                t = a + b;
                break;
            case 2:
                t = a - b;
                t = int.Parse(new string(t.ToString().Reverse().ToArray()));
                break;
            case 3:
                a = int.Parse(new string(a.ToString().Reverse().ToArray()));
                b = int.Parse(new string(b.ToString().Reverse().ToArray()));
                t = Mathf.Abs(a - b);
                break;
            case 4:
                t = a * b;
                if(t > 99999)
                     t = int.Parse(new string(t.ToString().Take(5).ToArray()));
                break;
            case 5:
                t = (a * b) % 100000;
                break;
            case 6:
                t = int.Parse((a % b).ToString() + (a / b).ToString());
                break;
            default:
                t = int.Parse((a / b).ToString() + (a % b).ToString());
                break;
        }
        Debug.Log(a + ", " + b + " = " + t);
        return t;
    }

    private void Setbuttons()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 p = bpos[i].localPosition;
            bpos[i].localPosition = new Vector3(p.x, entry[i] ? 0.0375f : -0.034f, p.z);
        }
        for (int i = 10; i < 14; i++)
        {
            Vector3 p = bpos[i].localPosition;
            bpos[i].localPosition = new Vector3(p.x, entry[10] ? 0.0375f : -0.034f, p.z);
        }
        bpos[15].localPosition = new Vector3(-0.0004626215f, entry[11] ? 0.0375f : -0.034f, 0.8358758f);
    }

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        enc = enc.Shuffle().ToList();
        Debug.LogFormat("[Dyscalculator #{0}] The true values of the digits are:\n[Dyscalculator #{0}] {1}", moduleID, string.Join("\n[Dyscalculator #" + moduleID + "] ", Enumerable.Range(0, 10).Select(x => x + " \u2192 " + enc[x]).ToArray()));
        ops = ops.Shuffle().ToArray();
        for (int i = 0; i < 4; i++)
            ops[i] += Random.Range(0, 2);
        Debug.LogFormat("[Dyscalculator #{0}] The true functions of the operators are:\n[Dyscalculator #{0}] {1}", moduleID, string.Join("\n[Dyscalculator #" + moduleID + "] ", Enumerable.Range(0, 4).Select(x => olog[x] + " \u2192 " + olog[ops[x] + 4]).ToArray()));
        int target = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(info.GetSerialNumber().First());
        int[] e = new int[3] { calc[target, 0], calc[target, 1], calc[target, 2] };
        Debug.LogFormat("[Dyscalculator #{0}] The target calculation is {1} {2} {3}", moduleID, e[0], olog[e[1]], e[2]);
        e = new int[3] { Errorcrypt(e[0], false), ops[e[1]], Errorcrypt(e[2], false) };
        ans = Calc(e[0], e[1], e[2]);
        Debug.LogFormat("[Dyscalculator #{0}] Applying errors: {1} {2} {3} = {4} \u2190 {5}", moduleID, e[0], olog[e[1] / 2], e[2], ans, Errorcrypt(ans, true));
        ans = Errorcrypt(ans, true);
        Debug.LogFormat("[Dyscalculator #{0}] Submit {1}", moduleID, ans);
        Setbuttons();
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if(!moduleSolved)
                {
                    if(b < 10)
                    {
                        if (entry[b])
                        {
                            button.AddInteractionPunch(0.1f);

                            if (operand)
                            {
                                if (!entry[12])
                                {
                                    entry[12] = true;
                                    disp[0].text = "";
                                }
                                if (disp[0].text.Length < 3)
                                {
                                    nums[2] *= 10;
                                    nums[2] += enc[b];
                                    disp[0].text += b.ToString();
                                    entry[b] = false;
                                    if(disp[0].text.Length >= 3)
                                    {
                                        for (int i = 0; i < 11; i++)
                                            entry[i] = false;
                                        entry[11] = true;
                                    }
                                    Setbuttons();
                                }
                            }
                            else
                            {
                                if (!entry[12])
                                {
                                    entry[12] = true;
                                    disp[0].text = "";
                                }
                                if(disp[0].text.Length >= 5)
                                {
                                    nums[0] %= 10000;
                                    disp[0].text = new string(disp[0].text.TakeLast(4).ToArray());
                                }
                                nums[0] *= 10;
                                nums[0] += enc[b];
                                disp[0].text += b.ToString();
                                entry[10] = disp[0].text.Distinct().Count() > 2;
                                entry[11] = true;
                                Setbuttons();
                            }
                        }
                    }
                    else if(b < 14)
                    {
                        if (entry[10])
                        {
                            button.AddInteractionPunch(0.1f);

                            entry[10] = false;
                            entry[12] = false;
                            operand = true;
                            string d = disp[0].text;
                            for (int i = 0; i < d.Length; i++)
                                entry[d[i] - '0'] = false;
                            entry[11] = false;
                            Setbuttons();
                            disp[1].text = olog[b - 10];
                            nums[1] = b - 10;
                        }
                    }
                    else if(b == 14)
                    {
                        button.AddInteractionPunch(0.1f);

                        operand = false;
                        nums = new int[3] { 0, 0, 0 };
                        entry = new bool[13] { true, true, true, true, true, true, true, true, true, true, false, false, false };
                        Setbuttons();
                        disp[0].text = "";
                        disp[1].text = "";
                    }
                    else if (entry[11])
                    {
                        button.AddInteractionPunch(0.5f);
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                        disp[1].text = "";
                        if (operand)
                        {
                            operand = false;
                            entry = new bool[13] { true, true, true, true, true, true, true, true, true, true, false, false, false };
                            Setbuttons();
                            int d = Calc(nums[0], ops[nums[1]], nums[2]);
                            disp[0].text = Errorcrypt(d, true).ToString();
                            Debug.LogFormat("[Dyscalculator #{0}] Query: {1} {2} {3} = {4}", moduleID, Errorcrypt(nums[0], true), olog[nums[1]], Errorcrypt(nums[2], true), disp[0].text);
                            nums = new int[3] { 0, 0, 0 };
                        }
                        else
                        {
                            Debug.LogFormat("[Dyscalculator #{0}] Submission: {1}", moduleID, disp[0].text);
                            if(disp[0].text == ans.ToString())
                            {
                                moduleSolved = true;
                                module.HandlePass();
                                for (int i = 0; i < 12; i++)
                                    entry[i] = false;
                                Setbuttons();
                                bpos[14].localPosition = new Vector3(0.7091364f, -0.034f, 0.5909242f);
                            }
                            else
                            {
                                module.HandleStrike();
                                nums = new int[3] { 0, 0, 0 };
                                entry = new bool[13] { true, true, true, true, true, true, true, true, true, true, false, false, false };
                            }
                            Setbuttons();
                            disp[0].text = "";
                        }
                    }
                }
                return false;
            };
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} enter <0-9> [Appends digits to operand] | !{0} A/S/M/D [Operations: Addition/Subtraction/Multiplication/Division] | !{0} query/submit | !{0} cancel";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        switch (command)
        {
            case "A":
            case "S":
            case "M":
            case "D":
                yield return null;
                if (entry[10])
                    buttons["ASMD".IndexOf(command) + 10].OnInteract();
                else
                    yield return "sendtochaterror!f Operators cannot be used at this time.";
                yield break;
            case "QUERY":
            case "SUBMIT":
                yield return null;
                if (operand ^ (command == "SUBMIT")) 
                    buttons[15].OnInteract();
                else if (operand)
                    yield return "sendtochaterror!f Use \"submit\" to submit the passcode.";
                else
                    yield return "sendtochaterror!f Use \"query\" to display calculation results.";
                yield break;
            case "CANCEL":
                yield return null;
                buttons[14].OnInteract();
                yield break;
            default:
                string[] commands = command.Split(' ');
                if (commands[0] == "ENTER")
                {
                    if (commands.Length > 1)
                    {
                        List<int> d = new List<int> { };
                        for (int i = 0; i < commands[1].Length; i++)
                        {
                            if ("0123456789".Contains(commands[1][i].ToString()))
                                d.Add(commands[1][i] - '0');
                            else
                            {
                                yield return "sendtochaterror!f " + commands[1][i] + " cannot be pressed.";
                                yield break;
                            }
                        }
                        for (int i = 0; i < d.Count(); i++)
                        {
                            yield return null;
                            buttons[d[i]].OnInteract();
                        } 
                    }
                }
                else
                    yield return "sendtochaterror!f " + commands[0] + "is an invalid command.";
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (operand || entry[12])
        {
            yield return null;
            buttons[14].OnInteract();
        }
        string a = ans.ToString();
        for(int i = 0; i < a.Length; i++)
        {
            yield return null;
            buttons[a[i] - '0'].OnInteract();
        }
        yield return null;
        buttons[15].OnInteract();
    }
}
