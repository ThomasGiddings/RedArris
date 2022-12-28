SELECT
  i.Name as InvestmentName,
  MAX(h.MarketValue)
FROM
  Investment i
  LEFT JOIN Holding h on i.Id = h.InvestmentId
  GROUP BY i.Id, i.Name