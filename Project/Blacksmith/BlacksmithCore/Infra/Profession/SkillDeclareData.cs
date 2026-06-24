using System;
using System.Text.RegularExpressions;

namespace BlacksmithCore.Infra.Profession
{
    public record class SkillDeclareData
    {
        public required string SkillName { get; init; }
        public required int Param { get; init; }
        public required string StringParam { get; init; }
        public required SkillDeclareData? Next { get; init; }

        public string ToDisplayString()
        {
            var parts = new List<string>();
            if (Param != 0)
                parts.Add($"p: {Param}");
            if (!string.IsNullOrEmpty(StringParam))
                parts.Add($"s: {StringParam}");

            var current = parts.Count > 0
                ? $"{SkillName}({string.Join(", ", parts)})"
                : SkillName;

            return Next != null
                ? $"{current} -> {Next.ToDisplayString()}"
                : current;
        }

        public static SkillDeclareData? Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 移除所有空格，简化后续解析
            string compact = Regex.Replace(input, @"\s+", "");
            if (compact.Length == 0)
                return null;

            // 按 "->" 拆分成多个技能片段
            string[] parts = compact.Split(new[] { "->" }, StringSplitOptions.None);
            if (parts.Length == 0)
                return null;

            SkillDeclareData? next = null;

            // 从后向前构建链表，方便设置 Next 引用
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part))
                    return null; // 不允许空技能名（例如连续的 "->" 或开头结尾的 "->"）

                // 提取技能名和参数部分
                string skillName;
                string? paramStr = null;

                int openParen = part.IndexOf('(');
                if (openParen >= 0)
                {
                    // 有括号时，必须严格匹配一对圆括号，且括号内不能再有括号
                    if (part[^1] != ')')
                        return null;
                    if (part.IndexOf('(', openParen + 1) >= 0)
                        return null;
                    if (part.IndexOf(')') != part.Length - 1)
                        return null;

                    skillName = part[..openParen];
                    paramStr = part[(openParen + 1)..^1]; // 去掉外层括号
                    // 注意：paramStr 可以为空（即 "()" 的情况），此时直接使用默认参数
                }
                else
                {
                    // 没有左括号，则不允许出现右括号
                    if (part.Contains(')'))
                        return null;
                    skillName = part;
                }

                // 技能名必须是非空且只包含字母数字，首字符为字母
                if (string.IsNullOrEmpty(skillName) || !Regex.IsMatch(skillName, @"^[A-Za-z][A-Za-z0-9]*$"))
                    return null;

                // 解析参数，默认值
                int param = 0;
                string stringParam = "";

                if (!string.IsNullOrEmpty(paramStr))
                {
                    // 按逗号分隔各键值对
                    string[] kvs = paramStr.Split(',');
                    bool hasP = false, hasS = false;

                    foreach (string kv in kvs)
                    {
                        if (string.IsNullOrEmpty(kv))
                            return null; // 不允许空参数项，例如 "p:1,,s:abc"

                        int colonIndex = kv.IndexOf(':');
                        if (colonIndex < 0)
                            return null; // 缺少冒号

                        string key = kv[..colonIndex];
                        string val = kv[(colonIndex + 1)..];

                        // 值不能为空（禁止 p: 或 s: 后无内容）
                        if (string.IsNullOrEmpty(val))
                            return null;

                        switch (key)
                        {
                            case "p":
                                if (hasP) return null;     // 重复的 p 参数
                                if (!int.TryParse(val, out param) || param < 0)
                                    return null;            // p 的值不是整数
                                hasP = true;
                                break;
                            case "s":
                                if (hasS) return null;     // 重复的 s 参数
                                // s 的值必须全部是字母或数字
                                if (!Regex.IsMatch(val, @"^[A-Za-z0-9]+$"))
                                    return null;
                                stringParam = val;
                                hasS = true;
                                break;
                            default:
                                return null;                // 未知的参数键
                        }
                    }
                }
                // 若括号内为空（paramStr 为 null 或 空），则直接使用默认的 param=0, stringParam=""

                // 构造当前节点，Next 指向上一次循环构造的节点
                var current = new SkillDeclareData
                {
                    SkillName = skillName,
                    Param = param,
                    StringParam = stringParam,
                    Next = next
                };
                next = current;
            }

            return next;
        }
    }
}