require "PageNode"

Class.CodeNode = {
	__inherit = "PageNode",

	parent = false, --parent node (example: method class)

	--syntax
	language = false, --language
	variants = false, --signatures
	attributes = false, --attributes associated with this node
	accessibility = "public", --"private" or "public"
	examples = false, --example code pieces

	--code reference
	file = false, --source file
	lines = false, --lines in the source file
}

function CodeNode:GenerateSeeAlsoLinks(writer)
	local references = { }

	if IsKindOf(self.parent, "PageNode") then
		table.insert(references, self.parent:GenerateLink(writer))
	end

	table.append(references, PageNode.GenerateSeeAlsoLinks(self, writer))

	return references
end

function CodeNode:GenerateExamples(writer)
	if not self.examples or not next(self.examples) then
		return ""
	end

	local examples = { }

	writer:ResetOrderedList()
	for i,example in ipairs(self.examples or empty_table) do
		table.insert(examples, writer:GenerateOrderedListLine("Example"))
		table.insert(examples, writer:GenerateSnippet(example, self.language, false))
	end

	return examples
end

function CodeNode:GenerateFooter(writer)
	local examples = self:GenerateExamples(writer)
	local references = PageNode.GenerateFooter(self, writer)

	return { examples, references }
end

function CodeNode:UpdateConnectionsWithNode(other)
	self:TryUpdateConnectionMember("parent", other)
end
