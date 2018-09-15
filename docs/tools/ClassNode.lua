require "CodeNode"

Class.ClassNode = {
	__inherit = "CodeNode",

	inherits = false, --inherited nodes
	methods = false, --method nodes
	properties = false, --properties nodes
	messages = false, --messages nodes
}

function ClassNode:GenerateSeeAlsoLinks(writer)
	local references = { }

	table.append(references, CodeNode.GenerateSeeAlsoLinks(self, writer))

	for i,parent in ipairs(self.inherits or empty_table) do
		if IsKindOf(parent, "PageNode") then
			table.insert(references, parent:GenerateLink(writer))
		end
	end

	return references
end

function ClassNode:GeneratePageContent(writer)
	local page = { }

	for i,variant in ipairs(self.variants or empty_table) do
		table.insert(page, writer:GenerateSection("Syntax"))
		table.insert(page, writer:GenerateSnippet(variant, self.language, self.summary))
	end

	if next(self.properties or empty_table) then
		table.insert(page, writer:GenerateSection("Members"))
		table.insert(page, writer:GenerateTable(self:GeneratePropertiesTable(writer)))
	end

	if next(self.methods or empty_table) then
		table.insert(page, writer:GenerateSection("Methods"))
		table.insert(page, writer:GenerateTable(self:GenerateMethodsTable(writer)))
	end

	if next(self.messages or empty_table) then
		table.insert(page, writer:GenerateSection("Messages"))
		table.insert(page, writer:GenerateTable(self:GenerateMessagesTable(writer)))
	end

	return page
end

-------------------------------

function ClassNode:GeneratePropertiesTable(writer)
	return self:GenerateTable(writer, self.properties)
end

function ClassNode:GenerateMethodsTable(writer)
	return self:GenerateTable(writer, self.methods)
end

function ClassNode:GenerateMessagesTable(writer)
	return self:GenerateTable(writer, self.messages)
end

function ClassNode:GenerateTable(writer, t)
	t = table.removed_duplicates(t)

	local result = { { "Name", "Description" } }
	for i,entry in ipairs(t or empty_table) do
		if IsKindOf(entry, "PageNode") then
			table.insert(result, { entry:GenerateLink(writer), entry.summary })
		elseif IsKindOf(entry, "Node") then
			table.insert(result, { entry.title, entry.summary })
		elseif type(entry) == "string" then
			table.insert(result, { entry, "..." })
		end
	end

	return result
end

function ClassNode:UpdateConnectionsWithNode(other)
	for i=1,#self.inherits do
		if self.inherits[i] == other.title then
			self.inherits[i] = other
		end
	end

	if other.parent == self then
		if IsKindOf(other, "MethodNode") then
			table.insert_unique(self.methods, other)
		elseif IsKindOf(other, "PropertyNode") then
			table.insert_unique(self.properties, other)
		elseif IsKindOf(other, "MessageNode") then
			table.insert_unique(self.messges, other)
		end
	end
end

