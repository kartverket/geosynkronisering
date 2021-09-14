namespace Provider_NetCore
{
    public enum Status
    {
        GET_LAST_TRANSNR,
        NO_CHANGES,
        HAS_CHANGES,
        GENERATE_CHANGES,
        GENERATE_CHANGES_FAILED,
        WRITE_CHANGES,
        WRITE_CHANGES_OK,
        UNKNOWN_ERROR
    }
}