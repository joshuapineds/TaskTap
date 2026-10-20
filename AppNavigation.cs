namespace TaskTap;

/// <summary>Application-level navigation helpers not tied to a single page.</summary>
public static class AppNavigation
{
    /// <summary>Returns to the landing page (Form1), closing any other open windows.</summary>
    public static void GoToLanding()
    {
        var landing = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        foreach (var form in Application.OpenForms.Cast<Form>().ToList())
        {
            if (form is not Form1)
                form.Close();
        }
        if (landing is null || landing.IsDisposed)
            landing = new Form1();
        landing.Show();
        landing.Activate();
        landing.BringToFront();
    }
}
