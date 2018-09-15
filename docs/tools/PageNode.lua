require "Node"

Class.PageNode = {
	__inherit = "Node",

	links = false, --links/references to other nodes
	article = false, --article this node belongs to

	file_path = false, --where the file will be saved
}

function PageNode:GenerateLink(writer, text)
	if self.file_path then
		return writer:GenerateLink(self.file_path, text or self.title)
	else
		return text
	end
end

function PageNode:GenerateCaption(writer)
	return { writer:GenerateCaption(self.title) }
end

function PageNode:GenerateFooter(writer)
	local references = { }

	if IsKindOf(self.article, "PageNode") then
		table.insert(references, self.article:GenerateLink(writer))
	end

	table.append(references, self:GenerateSeeAlsoLinks(writer))

	for i,link in ipairs(self.links or empty_table) do
		if IsKindOf(link, "PageNode") then
			table.insert(references, link:GenerateLink(writer))
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

	return self:ToString({ caption, content, footer })
end

function PageNode:GenerateSeeAlsoLinks(writer)
	return { }
end

function PageNode:GeneratePageContent()
	return { }
end

function PageNode:UpdateConnectionsWithNode(other)
	for i=1,#self.links do
		if self.links[i] == other.title then
			self.links[i] = other
		end
	end

	self:TryUpdateConnectionMember("article", other)
end
