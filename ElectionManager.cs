using MsgLib;
using MsgLib.Msg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessService
{
    public class ElectionManager
    {
        private Dictionary<string, CandidateDetails> m_Candidates = new Dictionary<string, CandidateDetails>();
        private Dictionary<string, Dictionary<string, VotesDetails>> m_Votes = new Dictionary<string, Dictionary<string, VotesDetails>>();

        public void HandleCandidateDetails(MsgBase msg)
        {
            CandidateDetails candidate = msg as CandidateDetails;
            if (!m_Candidates.ContainsKey(candidate.CandidateUID))
                m_Candidates.Add(candidate.CandidateUID, candidate);
        }

        public void HandleVotes(MsgBase msg)
        {
            VotesDetails vote = msg as VotesDetails;
            if (!m_Votes.ContainsKey(vote.State))
            {
                m_Votes.Add(vote.State, new Dictionary<string, VotesDetails>());
            }

            if (!m_Votes[vote.State].ContainsKey(vote.CandidateId))
                m_Votes[vote.State].Add(vote.CandidateId, vote);
            else
                m_Votes[vote.State][vote.CandidateId] = vote;
        }

        public List<MsgBase> HandleResoultRequest(MsgBase msg)
        {
            List<MsgBase> results = new List<MsgBase>();
            ElectionResultsRequest resultsRequest = msg as ElectionResultsRequest;
            if (resultsRequest.State == "")
            {
                ElectionResults result = new ElectionResults()
                {
                    RequestUID = resultsRequest.Base_MsgUID,
                    Election = resultsRequest.ElectionID,
                    State = ""
                };

                Dictionary<string, VotingResult> summaryResults = new Dictionary<string, VotingResult>();
                foreach (var state in m_Votes)
                {
                    foreach (VotesDetails votes in state.Value.Values)
                    {
                        if (summaryResults.ContainsKey(votes.CandidateId))
                        {
                            summaryResults[votes.CandidateId].VoteCount += votes.VoteCount;
                            summaryResults[votes.CandidateId].ElectoralVotesWon += votes.ElectoralVotesWon;
                        }
                        else
                        {
                            CandidateDetails candidate = m_Candidates[votes.CandidateId];
                            VotingResult vResult = new VotingResult()
                            {
                                CandidateId = votes.CandidateId,
                                CandidateFirstName = candidate.FirstName,
                                CandidateLastName = candidate.LastName,
                                Party = candidate.Party,
                                ElectoralVotes = 348,
                                ElectoralVotesWon = votes.ElectoralVotesWon,
                                VoteCount = votes.VoteCount,
                                FinalResults = false
                            };
                            summaryResults.Add(vResult.CandidateId, vResult);
                        }
                    }
                }
                int allVotes = summaryResults.Values.Sum(v => v.VoteCount);

                foreach (VotingResult vote in summaryResults.Values)
                {
                    vote.VotePercent = (vote.VoteCount / allVotes) * 100;
                    result.Voting.Add(vote);
                }

                results.Add(result);
            }
            else if (resultsRequest.State == "All")
            {
                foreach (var state in m_Votes)
                {
                    ElectionResults result = new ElectionResults()
                    {
                        RequestUID = resultsRequest.Base_MsgUID,
                        Election = resultsRequest.ElectionID,
                        State = state.Key
                    };

                    foreach (VotesDetails votes in state.Value.Values)
                    {
                        CandidateDetails candidate = m_Candidates[votes.CandidateId];
                        result.Voting.Add(new VotingResult()
                        {
                            CandidateId = votes.CandidateId,
                            CandidateFirstName = candidate.FirstName,
                            CandidateLastName = candidate.LastName,
                            Party = candidate.Party,
                            ElectoralVotes = votes.ElectoralVotes,
                            ElectoralVotesWon = votes.ElectoralVotesWon,
                            VoteCount = votes.VoteCount,
                            VotePercent = votes.VotePercent,
                            FinalResults = votes.FinalResults
                        });
                    }

                    results.Add(result);
                }
            }
            else
            {
                if (m_Votes.ContainsKey(resultsRequest.State))
                {
                    ElectionResults result = new ElectionResults()
                    {
                        RequestUID = resultsRequest.Base_MsgUID,
                        Election = resultsRequest.ElectionID,
                        State = resultsRequest.State
                    };

                    foreach (VotesDetails votes in m_Votes[resultsRequest.State].Values)
                    {
                        CandidateDetails candidate = m_Candidates[votes.CandidateId];
                        result.Voting.Add(new VotingResult()
                        {
                            CandidateId = votes.CandidateId,
                            CandidateFirstName = candidate.FirstName,
                            CandidateLastName = candidate.LastName,
                            Party = candidate.Party,
                            ElectoralVotes = votes.ElectoralVotes,
                            ElectoralVotesWon = votes.ElectoralVotesWon,
                            VoteCount = votes.VoteCount,
                            VotePercent = votes.VotePercent,
                            FinalResults = votes.FinalResults
                        });
                    }

                    results.Add(result);
                }
                else
                    results.Add(new ElectionResults());
            }

            return results;
        }
    }
}
