require "PageNode"

Class.ArticleNode = {
	__inherit = "PageNode",

	body = false,
	members = false,
}

function ArticleNode:__ctor()
	self.body = { }
	self.members = { }
end

function ArticleNode:GenerateSeeAlsoLinks(writer)
	local result = { }
	for i,node in ipairs(self.members) then
		table.insert(result, node:GenerateLink(writer))
	end

	return result
end

function ArticleNode:GeneratePageContent()
	return body
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

