namespace LSCore
{
    public class ReferencedFundText : FundText
    {
        public Get<FundText> reference;
        public float multiplier = 2;
        
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (reference == null) return;
            var value = reference.Data;
            
            if (value == this)
            {
                reference = null;
                return;
            }

            OnValidate();
            value.Validated -= OnValidate;
            value.Validated += OnValidate;

            void OnValidate()
            {
                Id = value.Id;
                Number = value.Number * multiplier;
            }
        }

        protected override void OnDestroy()
        {
            reference.Data.Validated -= OnValidate;
        }
#endif
    }
}