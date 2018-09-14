require "PageNode"

Class.ArticleNode = {
	__inherit = "PageNode",

	body = false,
}

function PageNode:GenerateSeeAlsoLinks(writer)
	local result = { }
	local graph = writer.current_graph
	if graph then
		for i,node in ipairs(graph.nodes) do
			if IsKindOf(node, "PageNode") and node.articel == self then
				table.insert(result, node:GenerateLink(writer))
			end
		end
	end

	return result
end

function ArticleNode:GeneratePageContent()
	return body
end
