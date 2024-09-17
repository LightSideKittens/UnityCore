using CronExpressionDescriptor;
using Cronos;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class CronExDrawer : OdinAttributeDrawer<CronExAttribute, string>
{
    private string minute = "*";
    private string hour = "*";
    private string dayOfMonth = "*";
    private string month = "*";
    private string dayOfWeek = "*";
    private bool show;
    private static GUIStyle style;

    protected override void Initialize()
    {
        base.Initialize();

        var split = ValueEntry.SmartValue.Split(" ");
        minute = split[0];
        hour = split[1];
        dayOfMonth = split[2];
        month = split[3];
        dayOfWeek = split[4];
    }
    

    protected override void DrawPropertyLayout(GUIContent label)
    {
        show = EditorUtils.DrawInBoxFoldout(label, Draw, this, show);
    }

    private void Draw()
    {
        Draw("Minute", ref minute);
        Draw("Hour", ref hour);
        Draw("Day of Month", ref dayOfMonth);
        Draw("Month", ref month);
        Draw("Day of Week", ref dayOfWeek);
        
        var cronExpression = $"{minute} {hour} {dayOfMonth} {month} {dayOfWeek}";

        try
        { 
            style ??= new GUIStyle(EditorStyles.label);
            style.wordWrap = true;
            
            CronExpression.Parse(cronExpression);
            ValueEntry.SmartValue = cronExpression;
            EditorGUILayout.SelectableLabel(cronExpression);
            EditorGUILayout.SelectableLabel(ExpressionDescriptor.GetDescription(cronExpression), style);
        }
        catch
        {
            EditorGUILayout.HelpBox("Invalid cron expression", MessageType.Error);
        }
    }

    private void Draw(string label, ref string val)
    {
        val = EditorGUILayout.TextField(label, val);
    }
}
