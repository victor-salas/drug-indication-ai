namespace DrugIndication.Domain.Models
{
    public class RawProgramInput
    {
        public int ProgramID { get; set; }

        public string ProgramName { get; set; } = string.Empty;

        public List<string> Drugs { get; set; } = new();

        public List<string> TherapeuticAreas { get; set; } = new();

        public string EligibilityDetails { get; set; } = string.Empty;

        public string ProgramDetails { get; set; } = string.Empty;

        public string ExpirationDate { get; set; } = string.Empty;

        public string ProgramUrl { get; set; } = string.Empty;

        public List<RawAssociatedFoundation> AssociatedFoundations { get; set; } = new();

        public string AnnualMax { get; set; } = string.Empty;

        public string MaximumBenefit { get; set; } = string.Empty;

        public string AddRenewalDetails { get; set; } = string.Empty;

        public string CouponVehicle { get; set; } = string.Empty;

        public string ActivationReq { get; set; } = string.Empty;

        public string ActivationMethod { get; set; } = string.Empty;

        public string FreeTrialOffer { get; set; } = string.Empty;

        public string OfferRenewable { get; set; } = string.Empty;
    }
}
