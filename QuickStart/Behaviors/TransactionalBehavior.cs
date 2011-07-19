using System.Transactions;
using FubuMVC.Core.Behaviors;

namespace QuickStart.Behaviors
{
    public class TransactionalBehavior : IActionBehavior
    {
        public IActionBehavior InnerBehavior { get; set; }
        public void Invoke()
        {
            using (var tx = new TransactionScope())
            {
                InnerBehavior.Invoke();
                tx.Complete();
            }
        }

        public void InvokePartial()
        {
            InnerBehavior.InvokePartial();
        }
    }
}