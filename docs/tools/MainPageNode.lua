require "PageNode"

Class.MainPageNode = {
	__inherit = "PageNode",

	maps = false,

	title = "Home page",
	file_path = "index.md.html",
}

function Node:GetCaption(writer)
	if writer.required_tags["private"] then
		return "[Internal] " .. self.title
	else
		return self.title
	end
end


function MainPageNode:GeneratePageContent(writer)
	local page = { }

	if self.summary and self.summary ~= "" then
		table.insert(page, writer:GenerateSection("Summary"))
		table.insert(page, self.summary)
	end

	if self.maps and next(self.maps) then
		table.insert(page, writer:GenerateSection("Documentation Maps"))
		local page_links = { }
		for i,map_node in ipairs(self.maps) do
			table.insert(page_links, self:GenerateLinkTo(writer, map_node))
		end
		table.insert(page, writer:GenerateList(page_links))
	end

	return page
end

function MethodNode:TryMergeNode(other)
	if not IsKindOf(other, "ArticleNode") then return end
	if other.title ~= self.title then return end

	self.summary = other.summary

	return true
end
