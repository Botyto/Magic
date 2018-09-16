require "PageNode"

Class.ArticleNode = {
	__inherit = "PageNode",

	members = false,
}

function ArticleNode:GetFilePrefix()
	return "article"
end

function ArticleNode:__ctor()
	self.members = { }
end

function ArticleNode:GenerateSeeAlsoLinks(writer)
	local result = { }
	for i,node in ipairs(self.members) do
		table.insert(result, self:GenerateLinkTo(writer, node))
	end

	return result
end

function ArticleNode:GeneratePageContent(writer)
	return {
		writer:GenerateSection("Article"),
		self.summary,
	}
end

function ArticleNode:UpdateConnectionsWithNode(other)
	if IsKindOf(other, "PageNode") then --other node has 'article' member
		if other.article == self.title or other.article == self then --it's this article
			local idx = table.find(self.members, other)
			if not idx then --and other node is not added already
				table.insert(self.members, other)
			end
		end
	end
end

