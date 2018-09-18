require "Node"

Class.PageNode = {
	__inherit = "Node",

	links = false, --links/references to other nodes
	article = false, --article this node belongs to

	file_path = false, --where the file will be saved
	root_path = false, --where the root is relative to `self.file_path`
}

function PageNode:GetFilePrefix()
	return ""
end

function PageNode:GenerateLinkTo(writer, node)
	assert(IsKindOf(node, "PageNode"))

	if not writer:ShouldOutputNode(node) then
		return
	end

	local to_root = self.root_path or ""
	local url = to_root .. node.file_path

	return writer:GenerateLink(url, node.title)
end

function Node:GetCaption(writer)
	return self.title
end

function PageNode:GenerateCaption(writer)
	return { writer:GenerateCaption(self:GetCaption(writer)) }
end

function PageNode:GenerateFooter(writer)
	local references = { }

	if IsKindOf(self.article, "PageNode") then
		table.insert(references, self:GenerateLinkTo(writer, self.article))
	end

	table.append(references, self:GenerateSeeAlsoLinks(writer))

	for i,link in ipairs(self.links or empty_table) do
		if IsKindOf(link, "PageNode") then
		table.insert(references, self:GenerateLinkTo(writer, link))
		end
	end

	if not next(references) then
		return ""
	end

	references = table.removed_duplicates(references)
	local section = writer:GenerateSection("See also")
	local links = writer:GenerateList(references)

	return { section, links }
end

function PageNode:GenerateContent(writer)
	local caption = self:GenerateCaption(writer)
	local content = self:GeneratePageContent(writer)
	local footer = self:GenerateFooter(writer)

	return self:ToString(writer, { caption, content, footer })
end

function PageNode:GenerateSeeAlsoLinks(writer)
	return { }
end

function PageNode:GeneratePageContent()
	return { }
end

function PageNode:UpdateConnectionsWithNode(other)
	for i=1,#(self.links or empty_table) do
		if self.links[i] == other.title then
			self.links[i] = other
		end
	end

	self:TryUpdateConnectionMember("article", other)
end
