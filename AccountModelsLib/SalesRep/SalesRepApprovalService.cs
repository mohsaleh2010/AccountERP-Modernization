using System;
using System.Linq;
using AccountModelsLib.SalesRep;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepApprovalService
    {
        private readonly operations _opp;

        public SalesRepApprovalService(operations opp)
        {
            _opp = opp;
        }

        public SalesRep_Approval RequestApproval(int type, Guid docGuid, long requestedBy, string reason)
        {
            var approval = new SalesRep_Approval
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Type = type,
                Ref_Document_Guid = docGuid,
                Requested_By = requestedBy,
                Reason = reason,
                Status = (int)SalesRepApprovalStatus.Pending
            };

            _opp.db.SalesRep_Approvals.Add(approval);
            _opp.db.SaveChanges();
            return approval;
        }

        public SalesRep_Approval CreateApprovalRequest(int type, string docGuid, long requestedBy, string reason)
        {
            return RequestApproval(type, Guid.Parse(docGuid), requestedBy, reason);
        }

        public void ApproveRequest(long approvalId, long approverId)
        {
            var approval = _opp.db.SalesRep_Approvals.FirstOrDefault(a => a.Id == approvalId);
            if (approval == null) throw new InvalidOperationException("طلب الموافقة غير موجود.");

            approval.Status = (int)SalesRepApprovalStatus.Approved;
            approval.Approver_Id = approverId;
            approval.Decided_At = DateTime.Now;
            _opp.db.SaveChanges();
        }
    }
}
