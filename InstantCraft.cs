namespace DSPCompleteReplicatorQueue
{
    public static class InstantCraft
    {
        // Preserve the original entry point; the request is serviced in small batches.
        public static void CompleteAllTasks(MechaForge forge)
        {
            if (Main.Instance != null) Main.Instance.Request(forge);
        }
    }
}
