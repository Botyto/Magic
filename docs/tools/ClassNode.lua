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

	table.insert(page, writer:GenerateSection("Members"))
	table.insert(page, writer:GenerateTable(self:GeneratePropertiesTable(writer)))

	table.insert(page, writer:GenerateSection("Methods"))
	table.insert(page, writer:GenerateTable(self:GenerateMethodsTable(writer)))

	table.insert(page, writer:GenerateSection("Messages"))
	table.insert(page, writer:GenerateTable(self:GenerateMessagesTable(writer)))

	return page
end

-------------------------------

function ClassNode:GeneratePropertiesTable(writer)
	return self:GenerateTable(self.properties)
end

function ClassNode:GenerateMethodsTable(writer)
	return self:GenerateTable(self.methods)
end

function ClassNode:GenerateMessagesTable(writer)
	return self:GenerateTable(self.messages)
end

function ClassNode:GenerateTable(writer, t)
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

	--methods, properties, messages should be set up during parsing
end

